using System;
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

        Verifier verifier;

        public Form1()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            ReadXls.onSheetChoise += this.getXlsSheet;
            loadInstruction = new LoadInstruction(this);
        }

        LoadInstruction loadInstruction;

        public string getXlsSheet(object[] sheetList)
        {
            DialogSheets form = new DialogSheets(sheetList);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
            string choosenSheet = form.sheet;
            if (choosenSheet == null)
            {
                choosenSheet = sheetList[0].ToString();
            }
            form.Dispose();
            return choosenSheet;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.Document.Body.MouseDown += new HtmlElementEventHandler(Body_MouseDown_Click);
        }

        void Body_MouseDown_Click(Object sender, HtmlElementEventArgs e)
        {
            switch (e.MouseButtonsPressed)
            {
                case MouseButtons.Left:
                    HtmlElement element = this.webBrowser1.Document.GetElementFromPoint(e.ClientMousePosition);
                    if (element != null && "button".Equals(element.GetAttribute("type"), StringComparison.OrdinalIgnoreCase))
                    {
                        if (element.Id == SMS.SMS_BUTTON_ID)
                        {
                            verifier.RuleAction.Execute();
                        }
                        else
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
                tables = new ReadXls(fileName);
                webBrowser1.DocumentText = $"<table><tr><td>file name:</td><td>{fileName}</td></tr><tr><td>sheet:</td><td>{tables.T1NAME}</td></tr><table>";
                instructionsToolStripMenuItem.Enabled = true;
                dgvTableXls.DataSource = null;
            }
        }

        void checkFile(string jsonFile)
        {

            if (!File.Exists(jsonFile))
            {
                string errorMessage = $"\r\nFile not found {jsonFile}";
                MessageBox.Show(errorMessage);
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

            verifier = new Verifier(schema, tables);
            dgvTableXls.DataSource = verifier.CheckTable;

            string htmlReport = createHtmlReport(tables.FileName);
            webBrowser1.DocumentText = htmlReport;
            BindingSource SBind = new BindingSource();
            SBind.DataSource = tables.t1; 
            dgvTableXls.DataSource = SBind;
        }

        string createHtmlReport(string xlsFile)
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
                            continue;
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

        private void whereButton_Click(object sender, EventArgs e)
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

        private void instructionsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem) {
                loadInstruction.updateInsrtuction((ToolStripMenuItem)sender);
            }
        }

        class LoadInstruction
        {
            public LoadInstruction(Form1 parent)
            {
                this.parent = parent;
                instructionFolder = $@"{Application.StartupPath}\{Properties.Settings.Default.InstructionFolder}";
            }

            public string instructionFolder { get; private set; }

            public string getFullName(string fileName)
            {
                return $"{instructionFolder}\\{fileName}";
            }

            Form1 parent;

            public void updateInsrtuction(ToolStripMenuItem menu)
            {
                menu.DropDownItems.Clear();
                addInstructionToMenu(menu
                    , getInsruction());
            }

            void addInstructionToMenu(ToolStripMenuItem parentMenu, string[] subMenuStrings)
            {
                foreach (string menu in subMenuStrings)
                {
                    parentMenu.DropDownItems.Add(Path.GetFileName(menu), null, instructionsMenuLoad_Click);
                }
            }

            string[] getInsruction()
            {
                if (Directory.Exists(instructionFolder))
                {
                    return Directory.GetFiles(instructionFolder, "*.json");
                }
                return new string[0];
            }

            private void instructionsMenuLoad_Click(object sender, EventArgs e)
            {
                if (sender is ToolStripMenuItem)
                {
                    ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                    parent.checkFile(getFullName(menu.Text));
                }
            }
        }

    }
}
