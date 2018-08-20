﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Web.Script.Serialization;
using Check.Properties;

namespace Check
{
    public partial class Form1 : Form
    {

        ReadXls tables;

        public Form1()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                //MessageBox.Show("Not enough parameters!");
                return;
            }
            string xlsFile = $@"{Path.GetDirectoryName(args[0])}\{args[1]}";

            //checkFile("C:\\Users\\IKotvytskyi\\Documents\\Visual Studio 2015\\checkReestr\\Check\\Check\\bin\\Release\\Форум_ Додаток 3 _СМАРТ КОЛЛЕКШН-1.xlsx");
            //checkFile("C:\\Users\\IKotvytskyi\\Documents\\Visual Studio 2015\\checkReestr\\Check\\Check\\bin\\Release\\skip 2430, 2429.new.tel.xls");
            //checkFile("C:\\Users\\IKotvytskyi\\Documents\\Visual Studio 2015\\checkReestr\\Check\\Check\\bin\\Release\\Skip Телефоны_2326.2327 .tel.xls");
            //checkFile("C:\\Users\\IKotvytskyi\\Documents\\Visual Studio 2015\\checkReestr\\Check\\Check\\bin\\Release\\Форум_ Додаток 3 _СМАРТ КОЛЛЕКШН.tel.xlsx");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.Document.Body.MouseDown += new HtmlElementEventHandler(Body_MouseDown);
        }

        void Body_MouseDown(Object sender, HtmlElementEventArgs e)
        {
            switch (e.MouseButtonsPressed)
            {
                case MouseButtons.Left:
                    HtmlElement element = this.webBrowser1.Document.GetElementFromPoint(e.ClientMousePosition);
                    if (element != null && "button".Equals(element.GetAttribute("type"), StringComparison.OrdinalIgnoreCase))
                    {
                        string[] fieldsArray = element.Id.Split('|');
                        if (fieldsArray.Length > 1)
                        {
                            txtColumnFilter.Text = fieldsArray[1];
                            dgvTableXls_changeColumns(txtColumnFilter.Text);
                            greedToClipboard();
                            MessageBox.Show(txtColumnFilter.Text);
                        }
                    }
                    break;
            }
        }

        void greedToClipboard()
        {
            this.dgvTableXls.ClipboardCopyMode =
                    DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dgvTableXls.SelectAll();
            if (this.dgvTableXls
                .GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    // Add the selection to the clipboard.
                    Clipboard.SetDataObject(
                        this.dgvTableXls.GetClipboardContent());
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    MessageBox.Show("The Clipboard could not be accessed. Please try again.");
                }
            }
        }


        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsb;*.xlsx";
            openFileDialog1.Title = "Select a Excel file";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                checkFile(fileName);
            }
        }

        void checkFile(string xlsFile)
        {

            string shemaPath =
                (Properties.Settings.Default.defaultShemaPatch == 0) ? 
                    Application.StartupPath : Path.GetDirectoryName(xlsFile);

            string jsonShortName = Path.GetFileNameWithoutExtension(xlsFile);

            if (jsonShortName.LastIndexOf('.') > 0 )
            {
                jsonShortName = jsonShortName.Remove(0, jsonShortName.LastIndexOf('.') + 1);
            }

            string jsonFile = $@"{shemaPath}\{jsonShortName}.json";

            string errorMessage = string.Empty;

            if (!File.Exists(jsonFile))
            {
                errorMessage = $"File not found : {jsonFile}";
                jsonFile = $"{shemaPath}\\{Properties.Settings.Default.defaultShemaFile}";
                if (!File.Exists(jsonFile))
                {
                    errorMessage += $"\r\nFile not found {jsonFile}";
                    MessageBox.Show(errorMessage);
                    return;
                }
            }

            if (!File.Exists(xlsFile))
            {
                MessageBox.Show($"File not found : {xlsFile}");
                return;
            }
            string jsonArray = File.ReadAllText(jsonFile);

            var serializer = new JavaScriptSerializer();
            dynamic schema;
            try
            {
                schema = serializer.DeserializeObject(jsonArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Open Json");
                return;
            }

            tables = new ReadXls(xlsFile);

            Verifier verifier = new Verifier(schema, tables);
            dgvTableXls.DataSource = verifier.CheckTable;

            string htmlReport = createHtmlReport(xlsFile, verifier);
            webBrowser1.DocumentText = htmlReport;
            //// ==>
            //tables.t1.WriteXml($@"{Path.GetDirectoryName(xlsFile)}\mytable.xml", XmlWriteMode.IgnoreSchema);
            ////===<

            BindingSource SBind = new BindingSource();
            SBind.DataSource = tables.t1; 
            dgvTableXls.DataSource = SBind;

        }

        string createHtmlReport(string xlsFile, Verifier verifier)
        {

            string style =
                @"
                    <style>
                    h1   {background-color: aqua; font-size: 1.2em;}
                    h2   {background-color: yellow; font-size: 1em;}
                    </style>
                 ";


            return
                    $@"
                    <html>
                    <head>
                    {style}
                    </head>
                    <body>
                    <h1>{xlsFile} </h1>
                    { String.Join("", verifier.report.ToArray())}
                    </body>
                    </html>
                ";
    
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (dgvTableXls.DataSource == null) return;
            dgvTableXls_changeColumns(txtColumnFilter.Text);
        }

        private void dgvTableXls_changeColumns(string filter)
        {
            if (filter == String.Empty)
            {
                foreach (DataGridViewColumn col in dgvTableXls.Columns)
                {
                    col.Visible = true;
                }
            }
            else
            {
                foreach (DataGridViewColumn col in dgvTableXls.Columns)
                {
                    bool res = false;
                    foreach (string str in filter.Split(';'))
                    {
                        if (str.Trim()=="")
                        {
                            break;
                        }
                        if (col.Name.ToLower().Contains(str.ToLower()))
                        {
                            res = true;
                            break;
                        }
                    }

                    if (res)
                    {
                        col.Visible = true;
                    }
                    else
                    {
                        col.Visible = false;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvTableXls.DataSource == null) return;
            try
            {
                (dgvTableXls.DataSource as BindingSource).Filter = txtResFilter.Text;
            }
            catch (Exception ex)
            {
                if (ex is EvaluateException || ex is EvaluateException)
                {
                    MessageBox.Show(ex.Message);
                }
                else throw;
            }

        }

        private void dgvTableXls_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((((DataGridView)sender).Name == "dgvTableXls")
                & (e.Button == MouseButtons.Right))
            {
                string name = dgvTableXls.Columns[e.ColumnIndex].Name;
                Clipboard.SetText(name);
                MessageBox.Show(String.Format("String \'{0}\' has been copied to clipboard", name));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set window location
            if (Settings.Default.mainFormLoaction != null)
            {
                this.Location= Settings.Default.mainFormLoaction;
            }

            // Set window size
            if (Settings.Default.mainFormSize != null)
            {
                this.Size = Settings.Default.mainFormSize;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Copy window location to app settings
            Settings.Default.mainFormLoaction = this.Location;

            // Copy window size to app settings
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.mainFormSize = this.Size;
            }
            else
            {
                Settings.Default.mainFormSize = this.RestoreBounds.Size;
            }

            // Save settings
            Settings.Default.Save();
        }


    }
}
