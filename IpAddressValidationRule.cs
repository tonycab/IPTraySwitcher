using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IPTraySwitcherWPF
{
 /// <summary>
 /// Class qui controle la validité d'une adresse IP
 /// </summary>
        public class IpAddressValidationRule : ValidationRule
        {
            public bool AllowEmpty { get; set; }

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                string input = (value ?? string.Empty).ToString().Trim();

                // Autoriser vide si optionnel
                if (string.IsNullOrEmpty(input))
                {
                    if (AllowEmpty)
                        return ValidationResult.ValidResult;
                    return new ValidationResult(false, "Champ requis");
                }

                // Vérifie si IP valide
                if (IPAddress.TryParse(input, out _))
                    return ValidationResult.ValidResult;

                return new ValidationResult(false, "Format adresse IP invalide");
            }
        }
    

}
