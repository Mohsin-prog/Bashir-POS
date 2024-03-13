using bashirPOS.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bashirPOS.View
{
    public partial class frmProductView : SampleView
    {
        public frmProductView()
        {
            InitializeComponent();
        }

        private void frmProductView_Load(object sender, EventArgs e)
        {
            GetData();
        }

        public void GetData()
        {
            //string qry = "Select pID, pName, pPrice, CategoryID, c.catName from  products p inner join category c on c.catID = p.CategoryID products where pName  like '%" + txtSearch.Text + "%'";
            string qry = "SELECT p.pID, p.pName, p.pPrice, p.CategoryID, c.catName FROM products p INNER JOIN category c ON c.catID = p.CategoryID WHERE p.pName LIKE '%" + txtSearch.Text + "%'";

            ListBox lb = new ListBox();
            lb.Items.Add(dgvid);
            lb.Items.Add(dgvName);
            lb.Items.Add(dgvPrice);
            lb.Items.Add(dgvcatID);
            lb.Items.Add(dgvcat);

            MainClass.LoadData(qry, guna2DataGridView1, lb);
        }

        public override void btnAdd_Click(object sender, EventArgs e)
        {

            MainClass.BlurBackground(new frmProductAdd());
            //frmCategoryAdd frm = new frmCategoryAdd();
            //frm.ShowDialog();
            GetData();
        }

        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            //Let's create table first
            GetData();
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dgvedit")
            {
                frmProductAdd frm = new frmProductAdd();
                frm.id = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dgvid"].Value);
                frm.cID = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dgvcatID"].Value);
                //frm.ShowDialog();
                MainClass.BlurBackground(frm);
                GetData();
            }
            //Modified this code to user the user to Delete data or not
            if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dgvdel")
            {
                // Ask the user for confirmation before deleting
                DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dgvid"].Value);
                    string qry = "DELETE FROM products WHERE pID = " + id + "";
                    Hashtable ht = new Hashtable();
                    MainClass.SQl(qry, ht);

                    MessageBox.Show("Deleted Successfully");
                    GetData(); // Refresh the data after deletion
                }
            }

            /* if (guna2DataGridView1.CurrentCell.OwningColumn.Name == "dgvdel")
             {
                 //Need to confirm before Deleting 
                 int id = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dgvid"].Value);
                 string qry = " Delete from category where catID = " + id + "";
                 Hashtable ht = new Hashtable();
                 MainClass.SQl(qry, ht);

                 MessageBox.Show("Deleted Successfully");
                 GetData();

             }*/
        }
       
    }
}
