using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CH_Client
{
    public partial class KILL : Form
    {
        public KILL()
        {
            InitializeComponent();
            this.ShowIcon = false;
        }

        private string Form2_value;
        public string Passvalue
        {
            get { return this.Form2_value; }
            set { this.Form2_value = value; }
        }

        private void KILL_Load(object sender, EventArgs e)
        {
            if(Passvalue == "3")
            {
                this.Close();
            }
            
        }

        private void checkBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void checkBox2_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void KILL_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Passvalue = "0";
            this.Dispose();
            this.Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            KILL_Program();
        }

        public void KILL_Program()
        {

            Passvalue = "1";

            this.Close();

        }
    }
}
