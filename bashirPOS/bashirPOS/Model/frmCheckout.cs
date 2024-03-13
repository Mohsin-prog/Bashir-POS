

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using bashirPOS.Reports;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;

namespace bashirPOS.Model
{
    public partial class frmCheckout : SampleAdd
    {
        public double amt { get; set; }
        public int MainID { get; set; }
    public frmCheckout()
        {
            InitializeComponent();
        }
        public void SetData(int mainID, double amount)
        {
            // Set the data in this form
            MainID = mainID;
            amt = amount;
        }
        
        private string selectedOrderType = "Take Away";
        
        

        // Event handler when the checkout form is loaded
        private void frmCheckout_Load(object sender, EventArgs e)
        {
            // Initialize the form controls and fields
            InitializeOrderTypeComboBox();
            UpdateCustomerFields();
            txtBillAmount.Text = amt.ToString();
        }

        // Initialize the order type ComboBox
        private void InitializeOrderTypeComboBox()
        {
            cmbOrderType.Items.Add("Take Away");
            cmbOrderType.Items.Add("Dine In");
            cmbOrderType.Items.Add("Delivery");
            cmbOrderType.SelectedItem = selectedOrderType;

            // Subscribe to the selection change event
            cmbOrderType.SelectedIndexChanged += cmbOrderType_SelectedIndexChanged;
        }

        // Event handler when order type is changed
        private void cmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update customer fields based on the selected order type
            selectedOrderType = cmbOrderType.SelectedItem.ToString();
            UpdateCustomerFields();
        }

        // Update the visibility of customer-related fields
        private void UpdateCustomerFields()
        {
            bool isDelivery = selectedOrderType == "Delivery";
            lblCustomerName.Enabled = isDelivery;
            lblCutomerAddress.Enabled = isDelivery;
            lblCustomerPhone.Enabled = isDelivery;
            txtCustomerName.Enabled = isDelivery;
            txtCustomerAddress.Enabled = isDelivery;
            txtCustomerPhone.Enabled = isDelivery;
        }    

        private void BtnSave_Click(object sender, EventArgs e)
        {
            
            try
            {
                double total;

                // Validate input for the "Total" field
                if (!double.TryParse(txtBillAmount.Text, out total))
                {
                    MessageBox.Show("Invalid input. Please enter a valid numeric value for Total.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(MainClass.con.ConnectionString))
                {
                    connection.Open();

                    // Start a new transaction
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {

                        try
                        {
                            string qry1 = "";
                            string qry2 = "";
                            int detailID = 0;

                            // Construct your main query based on the selected order type
                            if (selectedOrderType == "Take Away" || selectedOrderType == "Dine In")
                            {
                                qry1 = @"INSERT INTO tbMain (aDate, aTime, [status], orderType, total)
                                VALUES (@aDate, @aTime, @status, @orderType, @total);
                                SELECT SCOPE_IDENTITY()";
                            }
                            else if (selectedOrderType == "Delivery")
                            {
                                // Handle Delivery orders with customer details
                                string customerName = txtCustomerName.Text;
                                string customerAddress = txtCustomerAddress.Text;
                                string customerPhone = txtCustomerPhone.Text;

                                if (!string.IsNullOrEmpty(customerName) && !string.IsNullOrEmpty(customerAddress) && !string.IsNullOrEmpty(customerPhone))
                                {
                                    qry1 = @"INSERT INTO tbMain (aDate, aTime, [status], orderType, total, CustName, CustPhone, CustAddress)
                                    VALUES (@aDate, @aTime, @status, @orderType, @total, @CustName, @CustPhone, @CustAddress);
                                    SELECT SCOPE_IDENTITY()";
                                }
                                else
                                {
                                    MessageBox.Show("Invalid customer information. Please enter valid details for delivery orders.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            SqlCommand cmd = new SqlCommand(qry1, connection, transaction);
                            cmd.Parameters.AddWithValue("@aDate", DateTime.Now.Date);
                            cmd.Parameters.AddWithValue("@aTime", DateTime.Now.ToShortTimeString());
                            cmd.Parameters.AddWithValue("@status", "Pending");
                            cmd.Parameters.AddWithValue("@orderType", selectedOrderType);
                            cmd.Parameters.AddWithValue("@total", total);

                            if (selectedOrderType == "Delivery")
                            {
                                cmd.Parameters.AddWithValue("@CustName", txtCustomerName.Text);
                                cmd.Parameters.AddWithValue("@CustAddress", txtCustomerAddress.Text);
                                cmd.Parameters.AddWithValue("@CustPhone", txtCustomerPhone.Text);
                            }

                            if (MainID == 0)
                            {
                                MainID = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                            else
                            {
                                cmd.ExecuteNonQuery();
                            }

                            // Loop through DataGridView rows for order details
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                detailID = Convert.ToInt32(row.Cells["dgvid"].Value);

                                if (detailID == 0)
                                {
                                    qry2 = @"INSERT INTO tblDetails (MainID, proID, qt, price, amount)
                                     VALUES (@MainID, @proID, @qty, @price, @amount)";
                                }
                                else
                                {
                                    qry2 = @"UPDATE tblDetails SET proID = @proID, qt = @qty, price = @price, amount = @amount
                                     WHERE DetailID = @ID";
                                }

                                SqlCommand cmd2 = new SqlCommand(qry2, connection, transaction);
                                cmd2.Parameters.AddWithValue("@ID", detailID);
                                cmd2.Parameters.AddWithValue("@MainID", MainID);
                                cmd2.Parameters.AddWithValue("@proID", row.Cells["dgvproID"].Value);
                                cmd2.Parameters.AddWithValue("@qty", row.Cells["dgvQty"].Value);
                                cmd2.Parameters.AddWithValue("@price", row.Cells["dgvPrice"].Value);
                                cmd2.Parameters.AddWithValue("@amount", row.Cells["dgvAmount"].Value);

                                cmd2.ExecuteNonQuery();
                            }

                            // Commit the transaction
                            transaction.Commit();
                            MessageBox.Show("Save Successful");
                            // Don't set MainID to 0 here; it should retain the ID for further operations
                           
                          

                        }

                        catch (Exception ex)
                        {
                            // Rollback the transaction in case of an error
                            transaction.Rollback();
                            MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler when the "Print" button is clicked

        private void btnPrint_Click(object sender, EventArgs e)
        {
                
            try
            {
                // Check if MainID is valid
                if (MainID > 0)
                {
                    // Create a SQL query to fetch data for the report
                    string query = @"SELECT * FROM tbMain m 
                             INNER JOIN tblDetails d ON d.MainID = m.MainID 
                             INNER JOIN products p ON p.PID = d.ProID  
                             WHERE m.MainID = @MainID";

                    // Create a SqlConnection and SqlCommand
                    using (SqlConnection connection = new SqlConnection(MainClass.con.ConnectionString))
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add MainID as a parameter to the query
                        command.Parameters.AddWithValue("@MainID", MainID);

                        // Open the connection
                        connection.Open();

                        // Create a DataTable to hold the results
                    

                        // Use a SqlDataAdapter to fill the DataTable
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        // Fixed 'data' to 'da'

                        // Check if any data was retrieved
                        if (dataGridView1.Rows.Count > 0)
                        {
                            // Create an instance of your Crystal Report
                            rptBill cr = new rptBill();

                            // Set the report's data source
                            cr.SetDataSource(dataGridView1);

                            // Create an instance of the form to display the report
                            frmPrint frm = new frmPrint();

                            // Set the CrystalReportViewer's ReportSource to your report
                            frm.crystalReportViewer1.ReportSource = cr;
                            frm.crystalReportViewer1.Refresh();

                            // Show the form with the report
                            frm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("No data found for MainID " + MainID, "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("MainID is not set or invalid. Please save the data first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}

        //        public override void btnSave_Click(object sender, EventArgs e)
        //        {
        //            //Create a connection and a transaction using MainClass.con
        //using (SqlConnection connection = new  SqlConnection(MainClass.con.ConnectionString))
        //            {
        //                connection.Open();
        //                using (SqlTransaction transaction = connection.BeginTransaction())
        //                {
        //                    try
        //                    {
        //                        // Execute the main query to insert/update the main order
        //                        using (SqlCommand mainCmd = new SqlCommand(mainQuery, connection, transaction))
        //                        {
        //                            mainCmd.Parameters.AddWithValue("@ID", MainID);
        //                            mainCmd.Parameters.AddWithValue("@aDate", DateTime.Now.Date);
        //                            mainCmd.Parameters.AddWithValue("@aTime", DateTime.Now.ToShortTimeString());
        //                            mainCmd.Parameters.AddWithValue("@status", "Pending");
        //                            mainCmd.Parameters.AddWithValue("@orderType", cmbOrderType.SelectedItem.ToString());
        //                            mainCmd.Parameters.AddWithValue("@total", Convert.ToDouble(txtBillAmount.Text));

        //                            if (MainID == 0)
        //                            {
        //                                // Inserting a new order, get the new MainID
        //                                MainID = Convert.ToInt32(mainCmd.ExecuteScalar());
        //                            }
        //                            else
        //                            {
        //                                // Updating an existing order
        //                                mainCmd.ExecuteNonQuery();
        //                            }
        //                        }

        //                        // Execute detail queries to insert/update order details
        //                        foreach (DataGridViewRow row in guna2DataGridView1.Rows)
        //                        {
        //                            int detailID = Convert.ToInt32(row.Cells["dgvid"].Value);

        //                            if (detailID == 0) // Insert
        //                            {
        //                                detailQuery = "INSERT INTO tblDetails (MainID, proID, qt, price, amount) VALUES (@MainID, @proID, @qty, @price, @amount)";
        //                            }
        //                            else // Update
        //                            {
        //                                detailQuery = "UPDATE tblDetails SET proID = @proID, qt = @qty, price = @price, amount = @amount WHERE DetailID = @ID";
        //                            }

        //                            using (SqlCommand detailCmd = new SqlCommand(detailQuery, connection, transaction))
        //                            {
        //                                detailCmd.Parameters.AddWithValue("@ID", detailID);
        //                                detailCmd.Parameters.AddWithValue("@MainID", MainID);
        //                                detailCmd.Parameters.AddWithValue("@proID", row.Cells["dgvproID"].Value);
        //                                detailCmd.Parameters.AddWithValue("@qty", row.Cells["dgvQty"].Value);
        //                                detailCmd.Parameters.AddWithValue("@price", row.Cells["dgvPrice"].Value);
        //                                detailCmd.Parameters.AddWithValue("@amount", row.Cells["dgvAmount"].Value);

        //                                detailCmd.ExecuteNonQuery();
        //                            }
        //                        }

        //                        // Commit the transaction
        //                        transaction.Commit();
        //                        MessageBox.Show("Save Successful");
        //                        MainID = 0;
        //                        guna2DataGridView1.Rows.Clear();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        // If any error occurs, rollback the transaction
        //                        transaction.Rollback();
        //                        MessageBox.Show("An error occurred: " + ex.Message);
        //                    }
        //                }
        //        //{
        //        //    string qry1 = ""; //Main Table
        //        //    string qry2 = ""; // Detail Table

        //        //    int detailID = 0;

        //        //    if (MainID == 0)// Insert
        //        //    {
        //        //        qry1 = @"Insert into tbMain Values (@aDate, @aTime,@status, @orderType, @total)";
        //        //    }

        //        //    else //Update
        //        //    {
        //        //        qry2 = @"Update tbMainSet status = @status, total= @total, where MainID = @ID ";
        //        //    }

        //        //    Hashtable ht = new Hashtable();

        //        //    SqlCommand cmd = new SqlCommand(qry1, MainClass.con);
        //        //    cmd.Parameters.AddWithValue("ID", MainID);
        //        //    cmd.Parameters.AddWithValue("@aDate", DateTime.Now.Date);
        //        //    cmd.Parameters.AddWithValue("@aTime", DateTime.Now.ToShortTimeString());

        //        //    cmd.Parameters.AddWithValue("@status", "Pending");
        //        //    cmd.Parameters.AddWithValue("@orderType", cmbOrderType.SelectedItem.ToString());
        //        //    cmd.Parameters.AddWithValue("@total", Convert.ToDouble(txtBillAmount.Text)); // as we only saving data

        //        //    if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
        //        //    if (MainID == 0) { MainID == Convert.ToInt32(cmd.ExecuteScalar()); } else { cmd.ExecuteNonQuery(); }
        //        //    if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }


        //        //    foreach (DataGridViewRow row in guna2DataGridView1.Rows)
        //        //    {
        //        //        detailID = Convert.ToInt32(row.Cells["dgvid"].Value);

        //        //        if (detailID == 0) //Insert
        //        //        {
        //        //            qry2 = @"Insert into tblDetails Values (@MainID, @proID, @qty, @price, @amount)";
        //        //        }

        //        //        else //Update
        //        //        {
        //        //            qry2 = @"Update tblDetails Set @proID = proID, qt = @qty, price = @price, amount = @amount
        //        //                   where DetailID = @ID";
        //        //        }

        //        //        SqlCommand cmd2 = new SqlCommand(qry2,MainClass.con)
        //        //        cmd2.Parameters.AddWithValue("@ID", detailID);
        //        //        cmd2.Parameters.AddWithValue("@MainID", MainID);
        //        //        cmd2.Parameters.AddWithValue("@proID", row.Cells["dgvproID"]);
        //        //        cmd2.Parameters.AddWithValue("@qty", row.Cells["dgvQty"]);
        //        //        cmd2.Parameters.AddWithValue("@price", row.Cells["dgvPrice"]);
        //        //        cmd2.Parameters.AddWithValue("@amount", row.Cells["dgvAmount"]);

        //        //        if (MainClass.con.State == ConnectionState.Closed) { MainClass.con.Open(); }
        //        //        cmd2.ExecuteNonQuery();
        //        //        if (MainClass.con.State == ConnectionState.Open) { MainClass.con.Close(); }

        //        //        MessageBox.Show("Save Successfuly");
        //        //        MainID = 0;
        //        //        detailID = 0;
        //        //        guna2DataGridView1.Rows.Clear();

        //            }


        //try
        //{
        //    // Gather the data you need
        //    string orderType = cmbOrderType.SelectedItem.ToString();
        //    string CustName = txtCustomerName.Text;
        //    double CustPhone = Convert.ToDouble(txtCustomerPhone.Text);
        //    double amount = Convert.ToDouble(txtBillAmount.Text);

        //    // Construct your SQL query to save data to the 'tbMain' table
        //    string insertSql = @"INSERT INTO tbMain (total, orderType, CustName, CustPhone)
        //                 VALUES (@total, @orderType, @custName, @custPhone); 
        //                 SELECT SCOPE_IDENTITY();";

        //    // Create a Hashtable to store query parameters
        //    Hashtable ht = new Hashtable();
        //    ht.Add("@total", Convert.ToDouble(0));
        //    ht.Add("@orderType", orderType);
        //    ht.Add("@custName", CustName);
        //    ht.Add("@custPhone", Convert.ToDouble(0));

        //    // Execute the SQL query and retrieve the newly inserted MainID
        //    int mainID = Convert.ToInt32(MainClass.Scalar(insertSql, ht));

        //    if (mainID > 0)
        //    {
        //        MessageBox.Show("Data saved successfully with MainID: " + mainID, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        this.Close();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Error saving data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //}
    






    //private void FrmCheckout_Load(object sender, EventArgs e)
    //{
    //    txtBillAmount.Text = amt.ToString();
    //}

    //string qry = @" Update tbMain set totl = @total, received = @rec, change = @change 
    //           status = 'Paid' where MainID = @id";

    //Hashtable ht = new Hashtable();
    //ht.Add("@total", txtBillAmount.Text);
    //ht.Add("@rec", txtReceived.Text);
    //ht.Add("@change", txtChange.Text);

    //if (MainClass.SQl(qry, ht) > 0)
    //{
    //    MessageBox.Show("Saved Successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //    this.Close();

    //}
    