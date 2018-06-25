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

        public Verifier(string jsonShema, ITable iTable)
        {
            var serializer = new JavaScriptSerializer();
            schema = serializer.DeserializeObject(jsonShema);


            ICheckTable = iTable;
            ICheckTable.Open(schema[JOIN]);

            if ((schema is Dictionary<string, object>) && (schema as Dictionary<string, object>).ContainsKey(COMPARE))
            {
                compere(schema[COMPARE]);
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

        dynamic schema;

        public List<string> report { get; private set; } = new List<string>();

        void compere(object[] fieldsList)
        {
            report.Clear();
            foreach (object[] pair in fieldsList)
            {
                string ruleName = "";
                foreach (string name in pair)
                {
                    ruleName += $"{name};";
                }
                report.Add(ruleName);
                string[] result = ICheckTable.Select(pair);
                report.AddRange(result);
                report.Add("----------");
            }
        }
                

        /*
        void compere(object[] fieldsList)
        {
            report.Clear();
            int pairNumber = 0;
            foreach (object[] pair in fieldsList)
            {
                if (pair.Length == 2 )
                {
                    string columnName1 = pair[0].ToString();
                    string columnName2 = pair[1].ToString();
                    report.Add($"Pair: [{columnName1} -- {columnName2}]");
                    bool errors = false;
                    if (T1.Columns.Contains(columnName1) && T2.Columns.Contains(columnName2))
                    {
                        for (int rowNumber = 0; rowNumber < T1.Rows.Count; rowNumber++)
                        {
                            int xlsRow = rowNumber + FIRST_ROW;
                            if (T2.Rows.Count > rowNumber)
                            {
                                if (!T1.Rows[rowNumber][columnName1].Equals(T2.Rows[rowNumber][columnName2]))
                                {
                                    report.Add($"\tLine {xlsRow}: {T1.Rows[rowNumber][columnName1]} != {T2.Rows[rowNumber][columnName2].ToString()}");
                                    errors = true;
                                }
                            }
                            else
                            {
                                report.Add($"\tLine {xlsRow} not found");
                                errors = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        report.Add($"\tCan't found pair ({columnName1} -- {columnName2})");
                    }
                    if (errors)
                    {
                        report.Add("\r\n");
                    }
                    else
                    {
                        report.Add("\tOK!\r\n");
                    }
                }
                else
                {
                    string pairName = (pair.Length == 0) ? $"{1}..." :$"{pairNumber}.{pair[0].ToString()}" ;
                    report.Add($"Incorrect pair ({pairName})");
                }
                pairNumber++;
            }

        }
        */


    }
}
