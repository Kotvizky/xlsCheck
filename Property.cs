using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace Check
{
    class Property
    {
        public Property(DataTable addressTable, object[] _initObject)
        {
            initObject = _initObject;
            table = addressTable;
            initObject = _initObject;
            Exception exc;
            parser = new JsonParser(initObject);
            dynamic propertyParam = parser.getParam("property fields", out exc);
            dynamic parsing = parser.getParam("parsing", out exc);

            if (propertyParam is object[])
            {
                fieldNames = Array.ConvertAll((object[])propertyParam, x => x.ToString());
            }
            if ((propertyParam is object[]) && (parsing is Dictionary<string, object>))
            {
                fields = new Fields(fieldNames, parsing, new PropertyFiledsProcession());
                fields.addFieldsToTable(table);
            }

            dynamic addressName = parser.getParam("source string", out exc);

            if (addressName != null)
            {
                if (table.Columns.Contains(addressName.ToString()))
                {
                    sourceStringField = addressName.ToString();
                }
            }

            split();
        }

        void split()
        {
            if (sourceStringField == null)
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row[sourceStringField] != DBNull.Value)
                {
                    string[] values = new string[]{ row[sourceStringField].ToString()};
                    fields.addressToRow(row, values);
                }
            }
        }

        Fields fields;

        string sourceStringField;

        object[] initObject;

        static string[] fieldNames;

        static DataTable table;

        IJsonParser parser;

        class PropertyFiledsProcession : IFiledsProcession
        {
            public void setValues(string[] values, Fields fields)
            {
                foreach (Field field in fields)
                {
                    field.extractValue(values[0]);
                }

            }
        }

    }
}

