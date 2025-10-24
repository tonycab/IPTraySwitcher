using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms; // nécessite ajouter référence : System.Windows.Forms
using Application = System.Windows.Application;


namespace IPTraySwitcherWPF
{
    public partial class App : Application
    {
        private NotifyIcon _trayIcon;
        private ObservableCollection<Profile> _profiles;
        ContextMenuStrip menu;

        private string profileFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIIF", "IPTraySwitcher","profiles.json");
        private string iconFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.ico");

        public IEnumerable<string> Cartes { get; private set; }

        private void CreateMenu()
        {
            // Créer le menu contextuel
            if (menu == null) menu = new ContextMenuStrip();
            menu.Items.Clear();
            menu.Font = new Font("Cascadia Mono", 8);

            if (_profiles != null)
            {

                var padInterface = _profiles.OrderByDescending(p => p.Interface.Length).Select(p => p.Interface.Length).First();
                var padName = _profiles.OrderByDescending(p => p.Name.Length).Select(p => p.Name.Length).First();
                if (padName < "Automatique".Length) padName = "Automatique".Length;

                foreach (var p in _profiles)
                {
                    if (p.Dhcp)
                    {

                        menu.Items.Add($"{p.Interface.PadRight(padInterface)} | {"Automatique".PadRight(padName)} | (DHCP)", null, (s, ev) => SetDhcp(p.Interface, p.Name));
                    }

                    if (!p.Dhcp)
                    {

                        menu.Items.Add($"{p.Interface.PadRight(padInterface)} | {p.Name.PadRight(padName)} | ({p.IP})", null, (s, ev) =>
                            SetStaticIP(p.Interface, p.Name, p.IP, p.Mask, p.Gateway, p.DNS));
                    }

                    if (Cartes.Contains(p.Interface) == false)
                    {
                        var item = (ToolStripMenuItem)menu.Items[menu.Items.Count - 1];
                        item.Enabled = false;
                        item.ToolTipText = "Interface réseau non détectée";

                    }
                    else
                    {
                        var item = (ToolStripMenuItem)menu.Items[menu.Items.Count - 1];
                        item.Enabled = true;
                        if (p.Description != null) item.ToolTipText = p.Description;
                    }


                }

                menu.Items.Add("Ajouter des profils", null, (s, ev) =>
                {
                    var a = new ConfigView(_profiles, profileFile);
                    var r = a.ShowDialog();

                    CreateMenu();
                    menu.Refresh();

                });
                menu.Items.Add("Voir les cartes réseaux", null, (s, ev) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "ncpa.cpl",
                        UseShellExecute = true
                    });
                });


                menu.Items.Add(new ToolStripSeparator());

                menu.Items.Add("Quitter", null, (s, ev) => ExitApp());
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);

            // Liste les perifériques réseau
            Cartes = GetEthernet();

            // Charger les profils JSON s'il existe
            LoadProfiles(profileFile);

            // Creation du menu
            CreateMenu();

            // Créer l'icône système
            _trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(iconFile),
                Visible = true,
                Text = "IP Switcher",
                ContextMenuStrip = menu
            };
        }

        private void LoadProfiles(string jsonPath)
        {
            try
            {
                
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    var T_profiles = JsonSerializer.Deserialize<Profile[]>(json);
                    _profiles = new ObservableCollection<Profile>(T_profiles);
                }
                else
                {
                    var T_profiles = new[]
                    {
                        new Profile {Interface = "Ethernet",Dhcp=true, Name = "Dhcp", IP = "", Mask = "", Gateway = "", DNS = "" },
                        new Profile {Interface = "Ethernet",Dhcp=false, Name = "Automate", IP = "192.32.98.254", Mask = "255.255.255.0", Gateway = "192.32.98.1", DNS = "192.32.98.1" },
                        new Profile {Interface = "Ethernet",Dhcp=false, Name = "Camera", IP = "192.168.1.65", Mask = "255.255.255.0", Gateway = "192.168.1.1", DNS = "192.168.1.1" }
                    };
                    _profiles = new ObservableCollection<Profile>(T_profiles);

                    var f = Path.GetDirectoryName(jsonPath);
                    Directory.CreateDirectory(f);

                    File.WriteAllText(jsonPath, JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur chargement profils : " + ex.Message);
            }
        }

        private void SetDhcp(string interfaceName,string name)
        {
            try
            {
                RunNetsh($"interface ip set address \"{interfaceName}\" source=dhcp");
                RunNetsh($"interface ip set dns \"{interfaceName}\" source=dhcp");
                _trayIcon.ShowBalloonTip(2000, $"IP Switcher : {name}", "Mode DHCP activé ✅", ToolTipIcon.Info);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        private void SetStaticIP(string interfaceName, string name, string ip, string mask, string gateway, string dns)
        {
            try
            {
                RunNetsh($"interface ip set address name=\"{interfaceName}\" static {ip} {mask} {gateway} 1");
                RunNetsh($"interface ip set dns name=\"{interfaceName}\" static {dns}");
                _trayIcon.ShowBalloonTip(2000, $"IP Switcher : {name}", $"IP fixe appliquée : {ip}", ToolTipIcon.Info);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        private void RunNetsh(string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                
                Verb = "runas" // demande d'élévation admin
            };

            Process.Start(psi)?.WaitForExit();
        }

        private void ExitApp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Shutdown();
        }

        private IEnumerable<string> GetEthernet()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                Debug.WriteLine($"Nom : {ni.Name}");
                Debug.WriteLine($"Description : {ni.Description}");
                Debug.WriteLine($"Type : {ni.NetworkInterfaceType}");
                Debug.WriteLine($"Statut : {ni.OperationalStatus}");
                Debug.WriteLine($"Adresse MAC : {ni.GetPhysicalAddress()}");

                var ipProps = ni.GetIPProperties();
                foreach (var ip in ipProps.UnicastAddresses)
                {
                    Debug.WriteLine($"  IP : {ip.Address}");
                }

                Debug.WriteLine(new string('-', 40));
            }

            return interfaces.Select(ni => ni.Name);

        }



    }

}
