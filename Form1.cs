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
            //checkFile(xlsFile);


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
            tables = new ReadXls(xlsFile);
            string jsonFile = $@"{Path.GetDirectoryName(xlsFile)}\{Path.GetFileNameWithoutExtension(xlsFile)}.json";

            dataGridView1.DataSource = tables.t1;
            return;

            if (File.Exists(jsonFile))
            {
                textBox1.Clear();
                textBox1.Text = $"\t{xlsFile} "
                    + "\r\n=============\r\n";
                string jsonArray = File.ReadAllText(jsonFile);

                dataGridView1.DataSource = tables.t1;

                    //Verifier verifier = new Verifier(jsonArray, tables.t1, tables.t2);
                    //textBox1.Text += "\r\n" + String.Join("\r\n", verifier.report.ToArray());
            }
            else
            {
                MessageBox.Show($"File not found : {jsonFile}");
            }
        }

    }
}
