using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;

namespace bashirPOS.Model
{
    public partial class frmBillList : SampleAdd
    {
        private int MainID = 0;

        public frmBillList()
        {
            InitializeComponent();
        }

        private void frmBillList_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            string qry = "SELECT MainID, orderType, status, total FROM tbMain";

            try
            {
                using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(qry, con);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    MessageBox.Show("Rows retrieved: " + dt.Rows.Count.ToString());


                    // Add debugging statements to check the number of rows retrieved
                    MessageBox.Show("Rows retrieved: " + dt.Rows.Count.ToString());

                    guna2DataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        private void guna2DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int count = 0;
            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                count++;
                row.Cells["dgvid"].Value = count;
            }
        }

        private void guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "dgvedit")
                {
                    if (guna2DataGridView1.CurrentRow != null && guna2DataGridView1.CurrentRow.Cells["MainID"].Value != null)
                    {
                        MainID = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["MainID"].Value);
                        // Handle editing logic here or close the form.
                    }
                }
                else if (guna2DataGridView1.Columns[e.ColumnIndex].Name == "dgvdel")
                {
                    if (guna2DataGridView1.CurrentRow != null && guna2DataGridView1.CurrentRow.Cells["dgvid"].Value != null)
                    {
                        int MainID = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["dgvid"].Value);

                        // Your SQL query with proper joins and a parameterized query
                        string qry = @"
                            SELECT * FROM tbMain m 
                            INNER JOIN tblDetails d ON d.MainID = m.MainID 
                            INNER JOIN products p ON p.PID = d.ProID 
                            WHERE m.MainID = @MainID";

                        using (SqlConnection con = new SqlConnection(MainClass.con.ConnectionString))
                        using (SqlCommand cmd = new SqlCommand(qry, con))
                        {
                            cmd.Parameters.AddWithValue("@MainID", MainID);

                            try
                            {
                                con.Open();
                                DataTable dt = new DataTable();
                                SqlDataAdapter da = new SqlDataAdapter(cmd);
                                da.Fill(dt);

                                frmPrint frm = new frmPrint();
                                ReportDocument cr = new ReportDocument();

                                // Set the path to your Crystal Report file (*.rpt)
                                cr.Load("rptBill.rpt");

                                // Set the data source for your Crystal Report
                                cr.SetDataSource(dt);

                                // Set the Crystal Report Viewer's ReportSource to your Crystal Report
                                frm.crystalReportViewer1.ReportSource = cr;

                                // Refresh the Crystal Report Viewer
                                frm.crystalReportViewer1.Refresh();
                                frm.Show();
                            }
                            catch (Exception ex)
                            {
                                // Handle database or report generation errors here.
                                MessageBox.Show("Error: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
