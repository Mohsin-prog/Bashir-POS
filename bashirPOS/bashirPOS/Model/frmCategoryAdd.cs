﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bashirPOS.Model
{
    public partial class frmCategoryAdd : SampleAdd
    {
        public frmCategoryAdd()
        {
            InitializeComponent();
        }
        public int id = 0;
        public override void btnSave_Click(object sender, EventArgs e)
        {
            //adding Blue effect



            string qry = "";

            if (id == 0) //Insert
            {
                qry = "Insert into category Values(@Name)";
            }
            else //Update
            {
                qry = "Update category set catName = @Name where catID = @id";
            }

            Hashtable ht = new Hashtable();
            ht.Add("@id", id);
            ht.Add("@Name", txtName.Text);

           if (MainClass.SQl(qry, ht) > 0)
            {
                MessageBox.Show("Save Successfully...");
                id = 0;
                txtName.Text = "";
                txtName.Focus();
            }



        }
    }
}
