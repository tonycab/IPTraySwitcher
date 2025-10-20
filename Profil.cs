
using System;
using System.ComponentModel;
using System.Net;


    public class Profile : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _interface;
        private bool _dhcp;
        private string _name;
        private string _ip;
        private string _mask;
        private string _gateway;
        private string _dns;

        public string Interface
        {
            get => _interface;
            set { _interface = value; OnPropertyChanged(nameof(Interface)); }
        }

        public bool Dhcp
        {
            get => _dhcp;
            set
            {
                if (_dhcp != value)
                {
                    _dhcp = value;
                    OnPropertyChanged(nameof(Dhcp));

                    // Si DHCP activé → on vide les adresses statiques
                    if (_dhcp)
                    {
                        IP = string.Empty;
                        Mask = string.Empty;
                        Gateway = string.Empty;
                        DNS = string.Empty;
                    }
                }
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string IP
        {
            get => _ip;
            set { _ip = value; OnPropertyChanged(nameof(IP)); }
        }

        public string Mask
        {
            get => _mask;
            set { _mask = value; OnPropertyChanged(nameof(Mask)); }
        }

        public string Gateway
        {
            get => _gateway;
            set { _gateway = value; OnPropertyChanged(nameof(Gateway)); }
        }

        public string DNS
        {
            get => _dns;
            set { _dns = value; OnPropertyChanged(nameof(DNS)); }
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // --- IDataErrorInfo ---
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (Dhcp) return null; // Si DHCP activé → aucune validation requise

                switch (columnName)
                {
                    case nameof(IP):
                        if (string.IsNullOrWhiteSpace(IP))
                            return "L’adresse IP est requise.";
                        if (!IPAddress.TryParse(IP, out _))
                            return "Adresse IP invalide.";
                        break;

                    case nameof(Mask):
                        if (string.IsNullOrWhiteSpace(Mask))
                            return "Le masque est requis.";
                        if (!IPAddress.TryParse(Mask, out _))
                            return "Masque invalide.";
                        break;

                    case nameof(Gateway):
                        if (!string.IsNullOrWhiteSpace(Gateway) && !IPAddress.TryParse(Gateway, out _))
                            return "Passerelle invalide.";
                        break;

                    case nameof(DNS):
                        if (!string.IsNullOrWhiteSpace(DNS) && !IPAddress.TryParse(DNS, out _))
                            return "DNS invalide.";
                        break;
                }
                return null;
            }
        }
    }


