using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Check
{
    public partial class DialogSheets : Form
    {

        public string sheet;

        public DialogSheets(object[] sheetList)
        {
            InitializeComponent();
            listBox1.Items.AddRange(sheetList);
            listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sheet = listBox1.SelectedItem.ToString();
            Close();
        }
    }
}
