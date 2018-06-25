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
            string jsonFile = $@"{Path.GetDirectoryName(xlsFile)}\{Path.GetFileNameWithoutExtension(xlsFile)}.json";

            if (!File.Exists(jsonFile))
            {
                MessageBox.Show($"File not found : {jsonFile}");
            }

            if (!File.Exists(xlsFile))
            {
                MessageBox.Show($"File not found : {xlsFile}");
            }

            tables = new ReadXls(xlsFile);
            string jsonArray = File.ReadAllText(jsonFile);
            Verifier verifier = new Verifier(jsonArray, tables);
            dgvTableXls.DataSource = verifier.CheckTable;

            textBox1.Text += "\r\n" + String.Join("\r\n", verifier.report.ToArray());

            textBox1.Clear();
            textBox1.Text = $"\t{xlsFile} "
                + "\r\n=============\r\n";

            textBox1.Text += String.Join("\r\n", verifier.report.ToArray());
            dgvTableXls.DataSource = tables.t1;

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
    }
}
