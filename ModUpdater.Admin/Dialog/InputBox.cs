using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModUpdater.Admin.Dialog
{
    public partial class InputBox : Form
    {
        public string Value { get { return textBox1.Text; } }

        public InputBox(string helpText, string title)
        {
            InitializeComponent();
            label1.Text = helpText;
            Text = title;
            DialogResult = System.Windows.Forms.DialogResult.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }

        public static DialogResult Dialog(string helpText, string title, ref string output)
        {
            InputBox i = new InputBox(helpText, title);
            DialogResult r = i.ShowDialog();
            if (r == DialogResult.None || r == DialogResult.Cancel)
                return r;

            output = i.Value;
            return r;
        }
    }
}
