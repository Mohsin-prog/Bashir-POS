using bashirPOS.Model;
using bashirPOS.Reports;
using bashirPOS.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace bashirPOS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //accessing Form Main

        static Form1 _obj;
        public static Form1 Intance
        {
            get { if (_obj == null) { _obj = new Form1(); } return _obj; }
        }

        public static object Instance { get; internal set; }

        //Method to add Controls in Main File 

        public void AddControls(Form f)
        {
            ControlsPanel.Controls.Clear();
            f.Dock = DockStyle.Fill;
            f.TopLevel = false;
            ControlsPanel.Controls.Add(f);
            f.Show();
        }



        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            AddControls(new frmHome());
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            AddControls(new frmCategoryView());
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            _obj = this;
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            AddControls(new frmProductView());
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            frmPOS frm = new frmPOS();
            frm.Show();

        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Define the URL you want to open
            string url = "https://www.protechtians.com";

            try
            {
                // Use Process.Start to open the URL in the default web browser
                Process.Start(url);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                MessageBox.Show("Error opening the URL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            AddControls(new frmHistory());
        }
    }
}
