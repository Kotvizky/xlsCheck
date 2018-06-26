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

            if ((schema is Dictionary<string, object>) && (schema as Dictionary<string, object>).ContainsKey(SELECT))
            {
                select(schema[SELECT]);
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
        const string SELECT = "select";

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
