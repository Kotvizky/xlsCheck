using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Forms;

namespace Check
{
    static class Phone
    {

        /*
        public static void init(string[] _fields, DataRow row, char[] _separator)
        {
            fields = _fields;
            currentRow = row;
            separator = _separator;
        }
        */

        static OrderedDictionary fields;

        public static OrderedDictionary Fields
        {
            private get
            {
                return fields;
            }
            set
            {
                fields = value;
                fieldsKey = new object[fields.Count];
                fields.Keys.CopyTo(fieldsKey, 0);
            }
        }

        static object[] fieldsKey;

        public static char[] separator;

        public static DataRow currentRow;

        static void fillFields()
        {
            foreach (object key in fieldsKey)
            {
                object number = currentRow[key.ToString()];

                if ((number == null) || (valueExist(number) != 0))
                {
                    number = String.Empty;
                }
                fields[key] = number;
            }
        }
        
        static int valueExist(object value)
        {
            for (int i = 0; i < fields.Count; i++ )
            {
                if ((fields[i] != null) && (fields[i].Equals(value)))
                {
                    return i;
                }
            }
            return 0;
        }

        static void removeDouble()
        {
            for(int i = fields.Count - 1; i >= 0; i--)
            {
                if (fields[i].Equals(String.Empty))
                {
                    continue;
                }
                int newPosition = valueExist(fields[i]);
                if (newPosition < i)
                {
                    fields[i] = String.Empty;
                }
            }
        }

        static void fillRow()
        {

            for (int i = 0; i < fields.Count; i++)
            {
                fields[i] = correctNumber(fields[i].ToString());
            }

            removeDouble();

            foreach (DictionaryEntry entry in fields)
            {
                string number = fields[entry.Key].ToString();

                //fields.Cast<DictionaryEntry>().ElementAt(index)

                try
                {
                    currentRow[entry.Key.ToString()] = number;
                }
                catch (Exception e)
                {
                    if (e is ArgumentException)
                    {
                        if (number == String.Empty)
                        {
                            currentRow[entry.Key.ToString()] = DBNull.Value;
                        }
                        else
                        {
                            currentRow[entry.Key.ToString()] = Convert.ToDecimal(number);
                        }
                    }
                    else MessageBox.Show(e.Message);
                }
            }
        }

        public static string report;

        public static void splitPhones()
        {
            fillFields();
            report = String.Empty;
            for (int i = fields.Count -1 ; i >= 0 ; i--)
            {
                if ( fields[i].ToString() != String.Empty )
                {
                    List<string> correctPhones = getPhoneValues(i, fields[i].ToString());
                    if (correctPhones.Count > 1)
                    {
                        string correctPhonesString = String.Join(", ",correctPhones.ToArray());
                        report += $"[{fields[i].ToString()} -- {correctPhonesString}],";
                    }
                    writeFields(i,correctPhones);
                }
            }
            fillRow();
        }

        static void writeFields(int index, List<string> correctPhones)
        {
            if (correctPhones.Count == 0)
            {
                return;
            }
            int listIndex = 0;
            string sourceNumber = fields[index].ToString();
            for (int i = index; i < fields.Count ; i++)
            {
                if((i > index) && (fields[i].ToString() != String.Empty))
                {
                    continue;
                }
                if (listIndex == correctPhones.Count)
                {
                    return;
                }
                fields[i] = correctPhones[listIndex++];
            }
            string extraNumberts = String.Empty;
            while (listIndex < correctPhones.Count)
            {
                extraNumberts += $"{correctPhones[listIndex++]},";
            }
            if (extraNumberts != String.Empty)
            {
                report += $"[*del*{sourceNumber} -- {extraNumberts}]";
            }
        }

        static List<string> getPhoneValues(int index, string value)
        {
            string[] phones = value.Split(separator);
            List <string> phoneList = new List<string>();
            foreach(string phoneString in phones)
            {
                getPhoneFromString(phoneString,phoneList);
            }
            return phoneList;
        }

        static void getPhoneFromString(string numbers, List<string> correctPhones)
        {
            string number = new String(numbers.Where(Char.IsDigit).ToArray());
            //Regex.Match(numbers, @"\d+").Value;
            if (number != String.Empty)
            {
                correctPhones.Add(number);
            }
        }

        static string correctNumber(string number)
        {

            
            if ( ( number == String.Empty ) ||  (number.Length < 9) && (number.Length > 12) )
            {
                return number;
            }

            if (number.Length == 10)
            {
                number = $"8{number}";
            }
            else if ((number.Length == 9))
            {
                number = $"80{number}";
            }
            else if ((number.Length == 12) && (number.Substring(0,3) == "380"))
            {
                number = number.Substring(1, 11);
            }
            return number;
        }


    }

    //TODO fill all cell from current
    //TODO Procedure to fill: 
    //TODO 1. 

}
