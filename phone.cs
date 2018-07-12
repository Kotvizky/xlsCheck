using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

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

        public static string[] fields;

        public static char[] separator;

        public static DataRow currentRow;

        public static string report;


        public static void splitPhones()
        {
            report = String.Empty;
            for (int i = fields.Length -1 ; i >= 0 ; i--)
            {
                if ((currentRow[fields[i]] != DBNull.Value ) && ( currentRow[fields[i]].ToString() != "" ))
                {
                    List<string> correctPhones = getPhoneValues(i, currentRow[fields[i]].ToString());
                    string correctPhonesString = String.Join(",",correctPhones.ToArray());
                    report += $"[{currentRow[fields[i]].ToString()} -- {correctPhonesString}],";
                    writeToRow(i,correctPhones);
                }
            }
        }

        static void writeToRow(int index, List<string> correctPhones)
        {
            if (correctPhones.Count == 0)
            {
                return;
            }
            int listIndex = 0;
            for (int i = index; i < fields.Length - 1; i++)
            {
                if (listIndex == correctPhones.Count)
                {
                    return;
                }
                currentRow[fields[i]] = correctPhones[listIndex++];
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
            if (number != "")
            {
                correctPhones.Add(number);
            }
        }

    }

    //TODO fill all cell from current
    //TODO Procedure to fill: 
    //TODO 1. 

}
