using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Check
{
    class ReadXls : ITable
    {

        public ReadXls (string filename)
        {
            this.FileName = filename;
            initSheet();
        }

        public delegate string SheetChoice(object[] sheetList);

        public static event SheetChoice onSheetChoise;

        public DataTable table
        {
            get
            {
                return t1;
            }
        }

        public string FileName { get; private set; }
        public DataTable t1 { get; private set; }
        public DataTable t2 { get; private set; }
        string errors;
        public bool Exists {get; private set;} = false;

        public string T1NAME { get; private set; } = "in$";
        public const string T2NAME = "out$";

        void initSheet()
        {
            string connectionString = GetConnectionString();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;
                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                DataRow[] sheetRows = dtSheet.Select("TABLE_NAME like '*$''' or TABLE_NAME like '*$' ");
                if (sheetRows.Length > 0)
                {
                    if (sheetRows.Length == 1)
                    {
                        string sheetName = sheetRows[0]["TABLE_NAME"].ToString();
                    }
                    else
                    {
                        if (onSheetChoise != null)
                        {
                            object[] sheetList = new object[sheetRows.Length];
                            for (int i = 0; i < sheetRows.Length; i++)
                            {
                                T1NAME = sheetRows[i]["TABLE_NAME"].ToString();
                                sheetList[i] = T1NAME.Substring(0, T1NAME.Length - 1);
                            }
                            prepareSheetNames(sheetList);
                            T1NAME = onSheetChoise(sheetList);
                        }
                    }
                }
            }
        }

        void prepareSheetNames(object[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string sheet = names[i].ToString();
                if (sheet[0] == '\'' )
                {
                    names[i] = $@"{sheet}'";
                }
                else
                {
                    names[i] = $"{sheet}$";
                }
            }
        }


        private string GetConnectionString()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            props["Extended Properties"] = "Excel 12.0 XML";
            props["Data Source"] = FileName;
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }
            return sb.ToString();
        }

        public void Open(object[] join)
        {
            string connectionString = GetConnectionString();
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;
                // Get all Sheets in Excel File
                DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string table1 = (T1NAME[0] == '\'') ? $@"'{T1NAME}'" : T1NAME;
                string query = $"TABLE_NAME  in ('{table1}','{T2NAME}')";
                DataRow[] sheetRows = dtSheet.Select(query);
                if (sheetRows.Length == 2)
                {
                    t1 = GetTableFomFile(cmd, T1NAME,
                        $@"SELECT *
                        FROM [{ T1NAME}] t1 left join [{ T2NAME}] t2 
                            on (t1.[{join[0].ToString()}] = t2.[{join[1].ToString()}])"
                        );
                    Exists = true;
                }
                else if (sheetRows.Length == 1 )
                {
                    t1 = GetTableFomFile(cmd, T1NAME,
                        $@"SELECT *  FROM [{T1NAME}] "
                        );
                    Exists = true;
                }
                cmd = null;
                conn.Close();
                conn.Dispose();
            }
        }

        DataTable GetTableFomFile(OleDbCommand cmd, string sheetName, string Sql)
        {
            cmd.CommandText = Sql;
            DataTable XlsTable = new DataTable();
            XlsTable.TableName = sheetName;
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(XlsTable);
            return XlsTable;
        }


        public string[] Select(string filter, object[] fields = null)
        {
            try
            {

            DataRow[] rows = t1.Select(filter,"");
            int[] rowsId = new int[rows.Count()];
            for (int i = 0; i < rows.Count(); i++)
            {
                rowsId[i] = t1.Rows.IndexOf(rows[i]);
            }
            Array.Sort(rowsId);
            string[] result = new string[rows.Count()];
            for (int i = 0; i < rowsId.Count(); i++)
            {
                result[i] = $"{rowsId[i]} "; 
                if (fields != null)
                {
                    foreach (object field in fields)
                    {
                        result[i] += $"\t|\t{t1.Rows[rowsId[i]][field.ToString()]}";
                    }
                }
            }
            return result;
            }
            catch (Exception e )
            {
                if ((e is EvaluateException) || (e is ArgumentException))
                {
                    return new string[] { e.Message};
                }
                throw;
            }

        }

        public string[] Select(object[] filter, object[] fields = null)
        {
            string strFilter = $"[{filter[0].ToString()}] <> [{filter[1].ToString()}] or [{filter[1].ToString()}]  is NULL ";
            return Select(strFilter, (fields != null) ? fields : filter);
        }


    }
}
