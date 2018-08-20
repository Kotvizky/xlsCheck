using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Check
{
    static class Address
    {
        public static void Init(DataTable addressTable, object[] _initObject)
        {
            bool fieldsFlag = false;
            table = addressTable;
            initObject = _initObject;
            Exception exc;
            dynamic phoneParam = getParam("address fields", out exc);
            if (phoneParam is object[])
            {
                fieldNames = Array.ConvertAll( (object[])phoneParam, x => x.ToString());
                addFieldsToTable();
                field = new Fields(fieldNames);
            }
            dynamic addressName = getParam("address string", out exc);
            if (addressName != null)
            {
                if (table.Columns.Contains(addressName.ToString()))
                {
                    addressStringField = addressName.ToString();
                }
            }

            dynamic replaceParam = getParam("replace", out exc);
            if (replaceParam is object[])
            {
                replaceStrings = (object[])replaceParam;
            }

            replaceInAddress();

            split(getParam("split separators", out exc));

        }

        static string addressStringField = string.Empty;

        static object[] replaceStrings;

        static object[] initObject;

        static dynamic getParam(string paramName, out Exception exc)
        {
            exc = null;
            dynamic paramValue = null;
            foreach (dynamic jsonObj in initObject)
            {
                try
                {
                    paramValue = ((Dictionary<string, object>)jsonObj)[paramName];
                    break;
                }
                catch (Exception e)
                {
                    exc = e;
                }
            }
            return paramValue;
        }

        static string[] fieldNames;

        static Fields field;

        static void addFieldsToTable()
        {
            DataColumnCollection columns = table.Columns;
            foreach (string name in fieldNames)
            {

                if (!columns.Contains(name))
                {
                    table.Columns.Add(name,typeof(String));
                }
            }

        }

        static void replaceInAddress()
        {
            if ((addressStringField == string.Empty) || (replaceStrings == null) 
                || (!table.Columns[addressStringField].DataType.Equals(typeof(String))))
            {
                return;
            }
            foreach (DataRow row in table.Rows)
            {
                foreach (object pair in replaceStrings)
                {
                    if ((pair is object[]) && (((object[])pair).Length == 2))
                    {
                        string oldStr = ((object[])pair)[0].ToString();
                        string newStr = ((object[])pair)[1].ToString();
                        row[addressStringField] = row[addressStringField].ToString().Replace(oldStr, newStr);
                    }
                }
            }
        }

        static void split(object[] separators)
        {
            if (addressStringField == string.Empty)
            {
                return;
            }
            char[] chars = new char[separators.Length];
            for (int i = 0; i < separators.Length; i++)
            {
                if (separators[i].ToString() != "")
                {
                    chars[i] = separators[i].ToString()[0];
                }
            }
            foreach (DataRow row in table.Rows)
            {
                if (row[addressStringField] != DBNull.Value)
                {
                    //TODO split
                    string[] values = row[addressStringField].ToString().Split(
                            chars, fieldNames.Length, StringSplitOptions.RemoveEmptyEntries);

                    int diff = fieldNames.Length - values.Length;
                    for (int i = 0 ; i < values.Length ; i++) {
                        row[fieldNames[diff+i]] = values[i];
                        //row[fields[i]] = values[i];
                    }
                }
            }
        }

        static DataTable table;

    }

    class Fields: List<Field>
    {
        public Fields(string[] fieldNames)
        {
            foreach (string fieldName in fieldNames)
            {
                this.Add(new Field() { name = fieldName });
            }
        }
    }

    class Field {
        public string name;
        public bool isSet { get; private set; }
        public string[] regRuls;

    }

}
