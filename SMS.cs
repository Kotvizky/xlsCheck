using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;

namespace Check
{
    class SMS : IRulesAction 
    {

        public SMS(dynamic phoneParam, DataTable _table, List<string> _report, bool _add_380)
        {
            add_380 = _add_380;
            table = _table;
            report = _report;
            ready = false;
            try
            {
                smsPhones = findFieldsByMask(findObjects(phoneParam, "smsPhoneMask"));
                smsParam = findFieldsByList(findObjects(phoneParam, "fields"));
                if (smsPhones.Count == 0)
                {
                    ready = true;
                }
            }
            catch (Exception e)
            {
                report.Add(e.Message);
            }
        }

        static DataTable table;

        List<string> smsPhones;
        List<string> smsParam;

        bool ready = false;

        List<string> report;

        public void fillPhoneLines()
        {
            if (!ready)
            {
                message.Clear();
            }

            report.Add($"<h2>SMS phone split</h2>");
            report.Add($"<button id =\"{Check.SMS.SMS_BUTTON_ID}\">Copy SMS lines</button><hr>");
            report.Add("<pre>");
            int i = 0;
            foreach (DataRow row in table.Rows)
            {
                string str = SMS.GetSmsLine(row, smsPhones, smsParam);
                message.Add(str);
                if (i < 5)
                {
                    report.Add(str);
                    i++;
                }
            }
            report.Add("</pre>");
        }

        static object[] findObjects(dynamic jsonObj, string names)
        {
            return (((Dictionary<string, object>)jsonObj)[names] as object[]); ;
        }

        static List<string> findFieldsByMask(object[] fieldsMaskJson)
        {
            List<string> fields = new List<string>();
            foreach (string field in fieldsMaskJson)
            {
                foreach (DataColumn column in table.Columns)
                {
                    if (column.ColumnName.Contains(field) && (!field.Contains(column.ColumnName)))
                    {
                        fields.Add(column.ColumnName);
                    }
                }
            }
            return fields;
        }

        static List<string> findFieldsByList(object[] fieldsJson)
        {
            List<string> fields = new List<string>();
            foreach (string field in fieldsJson)
            {
                if (table.Columns.Contains(field))
                {
                    fields.Add(field);
                }
            }
            return fields;
        }



        readonly public static string SMS_BUTTON_ID = "sms_lines";

        List<string> message = new List<string>();

        public void Execute()
        {
            Clipboard.SetText(String.Join("",message.ToArray()));
            MessageBox.Show("SMS lines have copied to clipboard");
        }

        public static string GetSmsLine(DataRow row, List<string> smsPhones, List<string> smsParams)
        {
            string paramLine = string.Empty;
            string smsLine = string.Empty;
            Regex regex = new Regex("[0-9]");
            foreach (string param in smsParams)
            {
                string curParam = row[param].ToString();
                paramLine += $"\t{curParam}";
            }
            foreach (string smsNumber in smsPhones)
            {
                if ((row[smsNumber] != DBNull.Value) && (row[smsNumber].ToString().Length > 0))
                {
                    string curSmsNumber = row[smsNumber].ToString();
                    if (add_380 && (curSmsNumber.Length == 11) && (curSmsNumber.Substring(0, 2) == "80"))
                    {
                        curSmsNumber = "3" + curSmsNumber;
                    }
                    smsLine += $"{curSmsNumber}{paramLine}\r\n";
                }
            }
            return smsLine;
        }

        static bool add_380 = true;

    }
}
