using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IPTraySwitcherWPF
{
    /// <summary>
    /// Logique d'interaction pour ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {

        private string JsonPath = "Profiles.json";
        private ObservableCollection<Profile> Profiles { get; set; }
        public ObservableCollection<string> NetworkInterfaces { get; set; } = new();

        public ConfigView(ObservableCollection<Profile> _profiles, string fileConfig)
        {
            JsonPath = fileConfig;


            LoadNetworkInterfaces();

            Profiles = _profiles;


            InitializeComponent();

            DataContext = this;

            ProfilesGrid.ItemsSource = Profiles;
        }



        private void LoadNetworkInterfaces()
        {
            NetworkInterfaces.Clear();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Select(ni => ni.Name)
                .ToList();

            foreach (var iface in interfaces)
                NetworkInterfaces.Add(iface);

            if (NetworkInterfaces.Count == 0)
                NetworkInterfaces.Add("Aucune interface détectée");
        }

        private void LoadProfiles()
        {

            try
            {
                var o = new OpenFileDialog();

                o.Filter = "Fichier Json |*.json";

                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(o.FileName))
                    {
                        string json = File.ReadAllText(JsonPath);
                        var profiles = JsonSerializer.Deserialize<ObservableCollection<Profile>>(json);
                        Profiles.Clear();
                        foreach (var item in profiles)
                        {
                            Profiles.Add(item);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

        }

        private void SaveProfiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(Profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(JsonPath, json);
                System.Windows.MessageBox.Show("Profils sauvegardés !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var defaultInterface = NetworkInterfaces.FirstOrDefault() ?? "Ethernet";
            Profiles.Add(new Profile
            {
                Interface = defaultInterface,
                Dhcp = false,
                Name = "Nouveau Profil",
                IP = "",
                Mask = "255.255.255.0",
                Gateway = "",
                DNS = ""
            });
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesGrid.SelectedItem is Profile selected)
                Profiles.Remove(selected);
        }

        private void Load_Click(object sender, RoutedEventArgs e) => LoadProfiles();
        private void Save_Click(object sender, RoutedEventArgs e) => SaveProfiles();
    }
}
