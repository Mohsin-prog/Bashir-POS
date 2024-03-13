using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace bashirPOS.Reports
{
    public partial class frmHistory : SampleAdd
    {
        public frmHistory()
        {
            InitializeComponent();
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            cmbOrderType.Items.Add("All"); // Add an option to show all orders
            cmbOrderType.Items.Add("Takeaway");
            cmbOrderType.Items.Add("Dine-In");
            cmbOrderType.Items.Add("Delivery");

            // Set the default selection to "All"
            cmbOrderType.SelectedIndex = 0;

            LoadData();

        }
        private void LoadData()
        {
            string orderTypeFilter = cmbOrderType.SelectedItem as string;
            string qry = "SELECT MainID, CustName, CustPhone, orderType, total FROM tbMain";

            // Check if an order type filter is selected
            if (orderTypeFilter != "All")
            {
                // If not "All" selected, use the IN clause to filter multiple values
                qry += " WHERE orderType = @OrderType";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    if (orderTypeFilter != "All")
                    {
                        cmd.Parameters.AddWithValue("@OrderType", orderTypeFilter);
                    }

                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2DataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }



        //private void LoadData()
        //{
        //    string qry = "SELECT MainID, orderType, status, total FROM tbMain";

        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
        //        {
        //            con.Open();
        //            SqlDataAdapter adapter = new SqlDataAdapter(qry, con);
        //            DataTable dt = new DataTable();
        //            adapter.Fill(dt);

        //            guna2DataGridView1.DataSource = dt;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

    }
}