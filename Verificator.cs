using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Data;
using System.Text.RegularExpressions;

namespace Check
{
    class Verifier
    {

        public Verifier(dynamic _schema, ITable iTable)
        {
            schema = _schema;

             ICheckTable = iTable;
//            ICheckTable.Open(schema[JOIN]);

            if (runMethod(JOIN, ICheckTable.Open))
            {
                runMethod(COMPARE, compere);
                runMethod(SELECT, select);
            }

            if (runMethod(OPEN, ICheckTable.Open))
            {
                runMethod(IDROWS, inRows);
                runMethod(PHONE, phone);
                runMethod(REDEX, redex);
            }
        }

        delegate void method(object[] fieldsList);

        bool runMethod(string name, method method)
        {

            if ((schema is Dictionary<string, object>) && (schema as Dictionary<string, object>).ContainsKey(name))
            {
                method(schema[name]);
                return true;
            }
            else
            {
                return false;
            }
        }

        ITable ICheckTable;

        public DataTable CheckTable
        {
            get
            {
                return ICheckTable.table;
            }
        }

        const string COMPARE = "compare";
        const string JOIN = "join";
        const string OPEN = "open";
        const string SELECT = "select";
        const string PHONE = "phone";
        const string REDEX = "regex";
        const string IDROWS = "idRows";

        dynamic schema;

        public List<string> report { get; private set; } = new List<string>();

        void compere(object[] fieldsList)
        {
            int descPosition = 2;
            for (int i = 0; i < fieldsList.Count(); i++)
            {

                dynamic pair = fieldsList[i];

                if (IsNotArray(i, pair) || IsNotEnoughParam(i, descPosition, pair))
                {
                    continue;
                }

                string ruleName = $"\r\n[{COMPARE}]:\r\n{pair[0].ToString()};{pair[1].ToString()}";
                report.Add(ruleName);
                bool existReportFields = addFieldsToReport(pair, descPosition);
                string[] result = ICheckTable.Select(
                    new object[] { pair[0], pair[1] },
                    existReportFields ? pair[descPosition] : null
                    );
                report.AddRange(result);
                report.Add("----------");
            }
        }

        void select(object[] fieldsList)
        {
            for (int i = 0; i < fieldsList.Count(); i++)
            {

                dynamic pair = fieldsList[i];

                if (IsNotArray(i, pair) || IsNotEnoughParam(i, 1, pair))
                {
                    continue;
                }

                report.Add($"\r\n[{SELECT}]:\r\n{pair[0].ToString()}");

                bool existReportFields = addFieldsToReport(pair, 1);

                string[] result = ICheckTable.Select(
                    pair[0],
                    existReportFields ? pair[1] : null
                    );
                report.AddRange(result);
                report.Add("----------");
            }
        }

        void phone(object[] phoneObjects)
        {
            foreach (dynamic jsonObj in phoneObjects)
            {
                try
                {
                    dynamic phoneParam = ((Dictionary<string, object>)jsonObj)["split"];

                    string[] fieldsJson = Array.ConvertAll<object, string>((phoneParam["fields"] as object[]), x => x.ToString());

                    try
                    {

                        System.Collections.Specialized.OrderedDictionary fields 
                            = new System.Collections.Specialized.OrderedDictionary();
                        foreach (DataColumn column in ICheckTable.table.Columns)
                        {
                            if (fieldsJson.Contains(column.ColumnName))
                            {
                                fields.Add(column.ColumnName,null);
                            }
                        }
                        if (fields.Count == 0)
                        {
                            return;
                        }
                        Phone.Fields = fields;
                        Phone.separator = Array.ConvertAll<object, char>((phoneParam["symbols"] as object[]), x => Convert.ToChar(x));
                        report.Add($"<h2>Phone split</h2>");
                        foreach (DataRow row in ICheckTable.table.Rows)
                        {
                            Phone.currentRow = row;
                            Phone.splitPhones();
                            if (Phone.report != String.Empty)
                            {
                                report.Add(Phone.report + "<br>");
                            }
                        }
                    } catch(Exception e)
                    {
                        report.Add($"{e} -- {e.Message}");
                    }
                    break;
                }
                catch (Exception e)
                {
                }

            }

        }

        void redex(object[] regxObjects) {
            try
            {
                foreach (dynamic jsonObj in regxObjects)
                {
                    object[] fieldsJson = (((Dictionary<string, object>)jsonObj)["fields"] as object[] );
                    object[] isMatchJson = (((Dictionary<string, object>)jsonObj)["IsMatch"] as object[]);
                    object[] notMatchJson = (((Dictionary<string, object>)jsonObj)["NotMatch"] as object[]);
                    if ( (fieldsJson.Count() == 0) || (isMatchJson.Count() == 0) ) {
                        return;
                    }
                    List<string> fields = new List<string>();
                    DataTable table = ICheckTable.table;
                    foreach (string field in fieldsJson)
                    {
                        if (table.Columns.Contains(field))
                        {
                            fields.Add(field);
                        }
                    }

                    report.Add($"<h2>Redex</h2> <p>isMatch: {string.Join(" ; ", isMatchJson)}, notMatch: {string.Join(" ; ", notMatchJson)}<br>");
                    report.Add($"fields: {string.Join(";", fields)}</p><hr>");

                    if (fields.Count == 0 ) 
                    {
                        return;
                    }

                    List<RegexInv> match = new List<RegexInv>();
                    foreach( string rule in isMatchJson )
                    {
                        match.Add(new RegexInv(rule,false));
                    }

                    foreach (string rule in notMatchJson)
                    {
                        match.Add(new RegexInv(rule,true));
                    }

                    for (int rowNumber = 0; rowNumber < table.Rows.Count; rowNumber++)
                    {
                        DataRow row = table.Rows[rowNumber];
                        string rowReport = String.Empty;
                        foreach (string field in fields)
                        {
                            if ((row[field] != DBNull.Value) && (row[field].ToString() != String.Empty))
                            {
                                string value = row[field].ToString();
                                List<RegexInv> errorRules = match.FindAll(x => x.ExMatch(value));
                                if (errorRules.Count > 0)
                                {
                                    string[] errors = Array.ConvertAll<object, string>(errorRules.ToArray(), x => x.ToString());
                                    rowReport += $"<b>{value}</b> -- {string.Join(", ", errors)}; ";
                                }
                            }
                        }
                        if (rowReport != string.Empty)
                        {
                            report.Add($"# {rowNumber + 2}. {rowReport.Remove(rowReport.Length-2)} <br>");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                report.Add(e.Message);
            }
        }

        void inRows(object[] field)
        {
            try
            {
                if ((field.Count() == 1) && (field[0].ToString() != string.Empty))
                {
                    string fieldName = field[0].ToString();
                    DataTable table = ICheckTable.table;
                    if (table.Columns.Contains(fieldName)) {
                        table.Columns.Remove(table.Columns[fieldName]);
                    }
                    DataColumn Col = table.Columns.Add(fieldName, typeof(Int32));
                    Col.SetOrdinal(0);
                    for (int i = 0; i < table.Rows.Count; i++ )
                    {
                        table.Rows[i][fieldName] = i + 2;
                    }
                }
            } 
            catch (Exception e)
            {
                report.Add($"<pre>{e.Message}</pre>");
            }
        }

        bool addFieldsToReport(dynamic rules, int position)
        {
            bool result = ((rules as object[]).Count() > position  )
                    && (rules[position] is object[]);
            if (result)
            {
                string[] strFields = (rules[position] as object[]).Select(x => x.ToString()).ToArray();
                report.Add(String.Join(";", strFields));
            }
            return result;
        }

        private bool IsNotEnoughParam(int i, int paramNumber, dynamic pair)
        {
            bool result = (pair as object[]).Count() < paramNumber;
            if (result)
            {
                report.Add($"\r\n!{i + 1} - not enough parameters");
            }
            return result;
        }

        private bool IsNotArray(int i, dynamic pair)
        {
            bool result = !(pair is object[]);
            if (result)
            {
                report.Add($"\r\n!{i + 1} - not array of field");
            }
            return result;
        }

        class RegexInv : Regex
        {
            public RegexInv(string expretion, bool inversion) : base(expretion)
            {
                this.inversion = inversion;
            }
            public bool inversion { get; private set; }

            public bool ExMatch(string value)
            {
                return inversion ^ IsMatch(value);
            }

            public override string ToString()
            {
                string prefix = (inversion) ? "(-)" : "";
                return prefix + base.ToString();
            }

        };

    }
}
