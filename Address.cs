using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace Check
{
    static class Address
    {
        public static void Init(DataTable addressTable, object[] _initObject)
        {

            table = addressTable;
            initObject = _initObject;
            Exception exc;
            dynamic phoneParam = getParam("address fields", out exc);
            dynamic parsing = getParam("parsing", out exc);

            if (phoneParam is object[])
            {
                fieldNames = Array.ConvertAll( (object[])phoneParam, x => x.ToString());
                addFieldsToTable();
            }
            if ((phoneParam is object[]) && (parsing is Dictionary<string,object>))
            {
                fields = new Fields(fieldNames, parsing, new AddressFiledsProcession());
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

            dynamic replaceRegExParam = getParam("replace regex", out exc);
            if (replaceRegExParam is object[])
            {
                replaceRegExStrings = (object[])replaceRegExParam;
                replaceRegExInAddress();
            }

            split(getParam("split separators", out exc));
            trimValues();
        }

        static void trimValues()
        {
            foreach(DataRow row in table.Rows)
            {
                foreach (string fieldName in fieldNames)
                {
                    if (row[fieldName] != DBNull.Value)
                    {
                        row[fieldName] = row[fieldName].ToString().Trim();
                    }
                }
            }
        }

        static string addressStringField = string.Empty;

        static object[] replaceStrings;

        static object[] replaceRegExStrings;

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

        static Fields fields;

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
                        row[addressStringField] = Regex.Replace(row[addressStringField].ToString(),
                                oldStr, newStr);
                        //row[addressStringField] = row[addressStringField].ToString().Replace(oldStr, newStr);
                    }
                }
            }
        }

        static void replaceRegExInAddress()
        {
            if ((addressStringField == string.Empty) || (replaceRegExStrings == null)
                || (!table.Columns[addressStringField].DataType.Equals(typeof(String))))
            {
                return;
            }
            foreach (DataRow row in table.Rows)
            {
                foreach (object pair in replaceRegExStrings)
                {
                    if ((pair is object[]) && (((object[])pair).Length == 3))
                    {
                        string searchPattern = ((object[])pair)[0].ToString();
                        replacePattern = ((object[])pair)[1].ToString();
                        newValue = ((object[])pair)[2].ToString();
                        row[addressStringField] = Regex.Replace(row[addressStringField].ToString(), searchPattern, 
                            evaluator,RegexOptions.IgnorePatternWhitespace);
                    }
                }
            }
        }

        static string replacePattern;
        static string newValue;

        static MatchEvaluator evaluator = new MatchEvaluator(replaceInPattern);

        public static string replaceInPattern(Match match)
        {
            string value = match.Value;
            return Regex.Replace(value, replacePattern, newValue);
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
                    string[] values = row[addressStringField].ToString().Split(
                            chars, fieldNames.Length, StringSplitOptions.RemoveEmptyEntries);

                    fields.addressToRow(row,values);

                }
            }
        }

        static DataTable table;

        class AddressFiledsProcession : IFiledsProcession
        {
            public void setValues(string[] values, Fields fields)
            {
                for (int i = values.Length - 1; i >= 0; i--)
                {
                    foreach (Field field in fields)
                    {
                        if (!field.isSet)
                        {
                            if (field.setWithCheck(values[i]))
                            {
                                values[i] = string.Empty;
                                break;
                            }
                        }
                    }
                }

                for (int valNumer = values.Length - 1; valNumer >= 0; valNumer--)
                {
                    if ((values[valNumer] == string.Empty) || (values[valNumer].Trim() == ""))
                    {
                        continue;
                    }
                    for (int fieldNum = fields.Count - 3; fieldNum >= 0; fieldNum--) // building and appartment
                    {
                        if (!fields[fieldNum].isSet)
                        {
                            fields[fieldNum].value = values[valNumer];
                            values[valNumer] = string.Empty;
                            continue;
                        }
                    }

                }
            }
        }

    }

}
