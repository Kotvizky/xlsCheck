using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace Check
{
    class Fields : List<Field>
    {
        public Fields(string[] fieldNames, Dictionary<string, object> parsing, IFiledsProcession fieldsProcessind)
        {
            initFields = fieldsProcessind;

            foreach (string fieldName in fieldNames)
            {
                this.Add(new Field() { name = fieldName });
            }
            foreach (KeyValuePair<string, object> rule in parsing)
            {
                Field field = this.Find(x => x.name == rule.Key);
                if (field != null)
                {
                    field.regRuls = Array.ConvertAll((object[])rule.Value, x => x.ToString());
                }
            }
        }


        public void addFieldsToTable(DataTable table)
        {
            DataColumnCollection columns = table.Columns;
            foreach (Field field in this)
            {

                if (!columns.Contains(field.name))
                {
                    table.Columns.Add(field.name, typeof(String));
                }
            }
        }


        public void addressToRow(DataRow row, string[] values)
        {
            clearValues();
            initFields.setValues(values,this);
            writeFields(row);
        }

        public void clearValues()
        {
            foreach (Field field in this)
            {
                field.value = String.Empty;
            }
        }

        IFiledsProcession initFields;

        void writeFields(DataRow row)
        {
            foreach (Field field in this)
            {
                if (row.Table.Columns.Contains(field.name))
                {
                    row[field.name] = field.value;
                }
            }
        }
    }
}
