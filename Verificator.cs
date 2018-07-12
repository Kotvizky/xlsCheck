using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Data;


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
                runMethod(PHONE,phone);
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

                    List<string> correctFields = new List<string>();
                    foreach (DataColumn column in ICheckTable.table.Columns)
                    {
                        if (fieldsJson.Contains(column.ColumnName))
                        {
                            correctFields.Add(column.ColumnName);
                        }
                    }
                    if (correctFields.Count == 0)
                    {
                        return;
                    }

                    Phone.fields = correctFields.ToArray<string>();

                    Phone.separator = Array.ConvertAll<object, char>((phoneParam["symbols"] as object[]), x => Convert.ToChar(x));
                    foreach (DataRow row in ICheckTable.table.Rows)
                    {
                        Phone.currentRow = row;
                        Phone.splitPhones();
                        if (Phone.report != String.Empty)
                        {
                            report.Add(Phone.report);
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                }

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

    }
}
