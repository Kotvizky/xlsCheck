using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;


namespace Check
{
    class Field
    {
        public string name;

        public string value = String.Empty;

        public bool setWithCheck(string addressPart, bool shouldChec = true)
        {
            if (shouldChec && isSet)
            {
                return false;
            }

            foreach (string rule in regRuls)
            {
                Regex rx = new Regex(rule);
                if (rx.IsMatch(addressPart))
                {
                    value = addressPart;
                    return true;
                }
            }
            return false;
        }

        public void extractValue(string addressPart)
        {
            foreach (string rule in regRuls)
            {
                Regex rx = new Regex(rule);
                Match match =  rx.Match(addressPart);
                if (match.Groups.Count > 0)
                {
                    
                    value = (match.Groups["value"].Value  == "" ) ? 
                        match.Groups[match.Groups.Count - 1].Value : match.Groups["value"].Value;
                    addressPart = value;
                }
                else
                {
                    return;
                }
            }
        }

        public bool isSet
        {
            get
            {
                return (value != string.Empty);
            }
        }

        public string[] regRuls = new string[] { };
    }
}
