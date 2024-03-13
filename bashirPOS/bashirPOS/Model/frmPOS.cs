using bashirPOS.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace bashirPOS.Model
{
    public partial class frmPOS : Form
    {
        private int MainID = 0;
        private int savedMainID = 0;
        private double deliveryCharge = 0.0; // Added delivery charge field

        public frmPOS()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmPOS_Load(object sender, EventArgs e)
        {
            InitializeUI();
            txtdeliverycharges.KeyPress += new KeyPressEventHandler(txtdeliverycharges_KeyPress);

            LoadProducts();
        }

        private void InitializeUI()
        {
            guna2DataGridView1.BorderStyle = BorderStyle.FixedSingle;

            cmbOrderType.Items.AddRange(new string[] { "Takeaway", "Dine-In", "Delivery" });
            cmbOrderType.SelectedIndex = 0;

            CategoryPanel.Controls.Clear();
            AddCategory();
        }

        private void AddCategory()
        {
            string qry = "SELECT * FROM Category";
            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    Guna.UI2.WinForms.Guna2Button b = new Guna.UI2.WinForms.Guna2Button
                    {
                        Size = new Size(165, 45),
                        ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton,
                        Text = row["catName"].ToString()
                    };

                    // Set the initial FillColor
                    b.FillColor = Color.FromArgb(50, 55, 89);

                    // Handle the CheckedChanged event
                    b.CheckedChanged += (sender, e) =>
                    {
                        if (b.Checked)
                        {
                            // Change the FillColor when the button is checked
                            b.FillColor = Color.Red; // You can set your desired color here
                        }
                        else
                        {
                            // Change the FillColor when the button is unchecked
                            b.FillColor = Color.FromArgb(50, 55, 89); // Reset to the original color
                        }
                    };

                    // Add the button to your form or control
                    
                    b.Click += new EventHandler(b_Click);
                    CategoryPanel.Controls.Add(b);
                }
            }
        }

        private void b_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2Button b = (Guna.UI2.WinForms.Guna2Button)sender;
            foreach (var item in ProductPanel.Controls)
            {
                var pro = (ucProduct)item;
                pro.Visible = pro.PCategory.ToLower().Contains(b.Text.Trim().ToLower());
            }
        }

        private void AddItems(string proID, string name, string cat, string price, Image pimage)
        {
            var w = new ucProduct
            {
                PName = name,
                PPrice = price,
                PCategory = cat,
                PImage = pimage,
                id = Convert.ToInt32(proID)
            };

            ProductPanel.Controls.Add(w);

            w.onSelect += (ss, ee) =>
            {
                var wdg = (ucProduct)ss;
                bool productFound = false;

                foreach (DataGridViewRow item in guna2DataGridView1.Rows)
                {
                    if (Convert.ToInt32(item.Cells["dgvproID"].Value) == wdg.id)
                    {
                        item.Cells["dgvQty"].Value = int.Parse(item.Cells["dgvQty"].Value.ToString()) + 1;
                        item.Cells["dgvAmount"].Value = int.Parse(item.Cells["dgvQty"].Value.ToString()) *
                                                        double.Parse(item.Cells["dgvPrice"].Value.ToString());

                        productFound = true;
                        break;
                    }
                }

                if (!productFound)
                {
                    guna2DataGridView1.Rows.Add(new object[] { 0, 0, w.id, w.PName, 1, w.PPrice, w.PPrice });
                }

                GetTotal();
            };
        }

        private void LoadProducts()
        {
            string qry = "SELECT * FROM products INNER JOIN category ON catID = CategoryID";
            using (SqlCommand cmd = new SqlCommand(qry, MainClass.con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow item in dt.Rows)
                {
                    byte[] imageArray = (byte[])item["pImage"];
                    AddItems(item["pID"].ToString(), item["PName"].ToString(), item["catName"].ToString(),
                             item["pPrice"].ToString(), Image.FromStream(new MemoryStream(imageArray)));
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            foreach (var item in ProductPanel.Controls)
            {
                var pro = (ucProduct)item;
                pro.Visible = pro.PName.ToLower().Contains(txtSearch.Text.Trim().ToLower());
            }
        }

        private void guna2DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int count = 0;
            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                count++;
                row.Cells[0].Value = count;
            }
        }

        private void GetTotal()
        {
            double tot = 0;
            lblTotal.Text = "";
            foreach (DataGridViewRow item in guna2DataGridView1.Rows)
            {
                tot += double.Parse(item.Cells["dgvAmount"].Value.ToString());
            }
            lblTotal.Text = tot.ToString("N2");
        }
        private void ClearForm()
        {
            // Clear DataGridView
            guna2DataGridView1.Rows.Clear();

            // Reset MainID
            MainID = 0;

            // Clear textboxes and other controls as needed
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtAddress.Clear();
            txtdeliverycharges.Clear();
            lblTotal.Text = "0.00";

            // Reset combo box to the default selection
            cmbOrderType.SelectedIndex = 0;

            // Clear any other controls you want to reset
        }

        private void txtPhoneNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed key is not a digit (0-9) or control key
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Cancel the keypress event
            }
        }

        private void PhoneNumberTextBox_Enter(object sender, EventArgs e)
        {
            // When the TextBox gets focus, clear the placeholder text
            if (txtPhoneNumber.Text == "Enter phone number")
            {
                txtPhoneNumber.Text = "";
                txtPhoneNumber.ForeColor = SystemColors.WindowText; // Change the text color
            }
        }

        private void PhoneNumberTextBox_Leave(object sender, EventArgs e)
        {
            // When the TextBox loses focus and is empty, set the placeholder text
            if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
            {
                txtPhoneNumber.Text = "Enter phone number";
                txtPhoneNumber.ForeColor = SystemColors.GrayText; // Change the text color
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            guna2DataGridView1.Rows.Clear();
            MainID = 0;
            ClearForm();
            lblTotal.Text = "0.00";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string orderType = cmbOrderType.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(orderType))
            {
                MessageBox.Show("Please select an order type.");
                return;
            }

            double totalAmount = Convert.ToDouble(lblTotal.Text);
            // Check if there are any rows in the DataGridView
            if (guna2DataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Please select at least one product.");
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(MainClass.con.ConnectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (orderType == "Delivery" &&
                        (string.IsNullOrWhiteSpace(txtCustomerName.Text) ||
                         string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ||
                         string.IsNullOrWhiteSpace(txtAddress.Text)))
                            {
                                MessageBox.Show("Please fill in all customer details for delivery orders.");
                                return;
                            }
                            using (SqlCommand mainCmd = connection.CreateCommand())
                            {
                                mainCmd.Transaction = transaction;

                                if (MainID == 0)
                                {
                                    mainCmd.CommandText = @"INSERT INTO tbMain (aDate, aTime, [status], orderType, total, received, [change], CustName, CustPhone, CustAddress, deliverycharges)
                                             VALUES (@aDate, @aTime, @status, @orderType, @total, @received, @change, @CustName, @CustPhone, @CustAddress, @deliverycharges);
                                             SELECT SCOPE_IDENTITY()";
                                }
                                else
                                {
                                    mainCmd.CommandText = @"UPDATE tbMain
                                             SET [status] = @status, total = @total, received = @received, [change] = @change, deliverycharges = @deliverycharges
                                             WHERE MainID = @ID";
                                    mainCmd.Parameters.AddWithValue("@ID", MainID);
                                }

                                mainCmd.Parameters.AddWithValue("@aDate", DateTime.Now.Date);
                                mainCmd.Parameters.AddWithValue("@aTime", DateTime.Now.ToShortTimeString());
                                mainCmd.Parameters.AddWithValue("@status", "Pending");
                                mainCmd.Parameters.AddWithValue("@orderType", orderType);
                                mainCmd.Parameters.AddWithValue("@total", totalAmount);
                                mainCmd.Parameters.AddWithValue("@received", 0);
                                mainCmd.Parameters.AddWithValue("@change", 0);
                                mainCmd.Parameters.AddWithValue("@deliverycharges", deliveryCharge);

                                if (orderType == "Delivery")
                                {
                                    string customerName = txtCustomerName.Text.Trim();
                                    string phoneNumber = txtPhoneNumber.Text.Trim();
                                    string address = txtAddress.Text.Trim();

                                    mainCmd.Parameters.AddWithValue("@CustName", customerName);
                                    mainCmd.Parameters.AddWithValue("@CustPhone", phoneNumber);
                                    mainCmd.Parameters.AddWithValue("@CustAddress", address);
                                }
                                else
                                {
                                    mainCmd.Parameters.AddWithValue("@CustName", "");
                                    mainCmd.Parameters.AddWithValue("@CustPhone", "");
                                    mainCmd.Parameters.AddWithValue("@CustAddress", "");
                                }

                                if (MainID == 0)
                                {
                                    MainID = Convert.ToInt32(mainCmd.ExecuteScalar());
                                    savedMainID = MainID;
                                }
                                else
                                {
                                    mainCmd.ExecuteNonQuery();
                                }
                            }

                            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                            {
                                int detailID = Convert.ToInt32(row.Cells["dgvID"].Value);

                                using (SqlCommand detailCmd = connection.CreateCommand())
                                {
                                    detailCmd.Transaction = transaction;

                                    if (detailID == 0)
                                    {
                                        detailCmd.CommandText = @"INSERT INTO tblDetails (MainID, proID, qty, price, amount)
                                          VALUES (@MainID, @proID, @qty, @price, @amount)";
                                    }
                                    else
                                    {
                                        detailCmd.CommandText = @"UPDATE tblDetails
                                          SET proID = @proID, qty = @qty, price = @price, amount = @amount 
                                          WHERE DetailID = @ID";
                                        detailCmd.Parameters.AddWithValue("@ID", detailID);
                                    }

                                    detailCmd.Parameters.AddWithValue("@MainID", MainID);
                                    detailCmd.Parameters.AddWithValue("@proID", Convert.ToInt32(row.Cells["dgvproID"].Value));
                                    detailCmd.Parameters.AddWithValue("@qty", Convert.ToInt32(row.Cells["dgvQty"].Value));
                                    detailCmd.Parameters.AddWithValue("@price", Convert.ToDouble(row.Cells["dgvPrice"].Value));
                                    detailCmd.Parameters.AddWithValue("@amount", Convert.ToDouble(row.Cells["dgvAmount"].Value));

                                    detailCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Saved Successfully");
                            guna2DataGridView1.Rows.Clear();
                            lblTotal.Text = "0.00";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            ClearForm();
            RefreshForm();
        }


        private bool deliveryChargeChanged = false; // Add a flag to track changes

        private void txtdeliverycharges_TextChanged(object sender, EventArgs e)
        {
            if (!deliveryChargeChanged) // Check if the user hasn't made changes yet
            {
                txtdeliverycharges.Text = ""; // Clear the placeholder text
                deliveryChargeChanged = true; // Set the flag to true
            }

            if (double.TryParse(txtdeliverycharges.Text, out deliveryCharge))
            {
                UpdateTotalAmount();
            }
            else
            {
                deliveryCharge = 0.0;
                UpdateTotalAmount();
            }
        }


        private void UpdateTotalAmount()
        {
            double totalAmount = 0.0;

            if (cmbOrderType.SelectedItem?.ToString() == "Delivery")
            {
                totalAmount += deliveryCharge;
            }

            foreach (DataGridViewRow row in guna2DataGridView1.Rows)
            {
                totalAmount += Convert.ToDouble(row.Cells["dgvAmount"].Value);
            }

            lblTotal.Text = totalAmount.ToString("N2");
        }

        private void RefreshForm()
        {
            guna2DataGridView1.Rows.Clear();
            lblTotal.Text = "0.00";
            MainID = 0;
        }

       

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            try
            {
                string latestMainIDQuery = @"SELECT TOP 1 MainID FROM tbMain ORDER BY MainID DESC";

                using (SqlConnection connection = new SqlConnection(MainClass.con.ConnectionString))
                using (SqlCommand latestMainIDCommand = new SqlCommand(latestMainIDQuery, connection))
                {
                    connection.Open();

                    int latestMainID = Convert.ToInt32(latestMainIDCommand.ExecuteScalar());

                    if (latestMainID > 0)
                    {
                        string query = @"
                        SELECT m.MainID, m.aDate, m.aTime, m.orderType, m.CustName, m.CustPhone, m.CustAddress, m.deliverycharges, m.total,
                               d.qty, d.price, d.amount, p.PName
                        FROM tbMain m
                        INNER JOIN tblDetails d ON d.MainID = m.MainID
                        INNER JOIN products p ON p.PID = d.proID
                        WHERE m.MainID = @MainID
                        AND m.MainID = (
                            SELECT TOP 1 MainID
                            FROM tbMain
                            WHERE MainID = @MainID
                            ORDER BY MainID DESC
                        )";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MainID", latestMainID);

                            DataTable reportData = new DataTable();
                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(reportData);

                            if (reportData.Rows.Count > 0)
                            {
                                rptBill cr = new rptBill();
                                cr.SetDataSource(reportData);

                                PrintDialog printDialog = new PrintDialog();
                                printDialog.Document = cr.PrintOptions.PrinterName == null ? new PrintDocument() : new PrintDocument();
                                printDialog.UseEXDialog = true;

                                if (printDialog.ShowDialog() == DialogResult.OK)
                                {
                                    cr.PrintOptions.PrinterName = printDialog.PrinterSettings.PrinterName;
                                    cr.PrintToPrinter(1, false, 0, 0);
                                    MessageBox.Show("Receipt printed successfully.", "Print Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("No data found for MainID " + latestMainID, "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                        }
                    }
               }
            }
                    
            //            using (SqlCommand command = new SqlCommand(query, connection))
            //            {
            //                command.Parameters.AddWithValue("@MainID", latestMainID);

            //                DataTable reportData = new DataTable();
            //                SqlDataAdapter da = new SqlDataAdapter(command);
            //                da.Fill(reportData);

            //                if (reportData.Rows.Count > 0)
            //                {
            //                    rptBill cr = new rptBill();
            //                    cr.SetDataSource(reportData);

            //                    frmPrint frm = new frmPrint();
            //                    frm.crystalReportViewer1.ReportSource = cr;
            //                    frm.crystalReportViewer1.Refresh();
            //                    frm.ShowDialog();
            //                }
            //                else
            //                {
            //                    MessageBox.Show("No data found for MainID " + latestMainID, "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            MessageBox.Show("No records found in tbMain.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }
            //    }
            //}
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void cmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isDelivery = cmbOrderType.SelectedItem?.ToString() == "Delivery";
            txtCustomerName.Enabled = isDelivery;
            txtPhoneNumber.Enabled = isDelivery;
            txtAddress.Enabled = isDelivery;
            txtdeliverycharges.Enabled = isDelivery;

            // Clear the placeholders when Takeaway or Dine-In is selected
            if (!isDelivery)
            {
                txtCustomerName.Text = "";
                txtPhoneNumber.Text = "";
                txtAddress.Text = "";
                txtdeliverycharges.Text = "0"; // You can set this to your default delivery charge
            }

            UpdateTotalAmount();
        }

        private void btnRevertSelection_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> rowsToRemove = new List<DataGridViewRow>();

            // Loop through the DataGridView and decrement the quantity by 1 for selected items
            foreach (DataGridViewRow row in guna2DataGridView1.SelectedRows)
            {
                // Check if the row is selected
                if (row.Selected)
                {
                    int currentQty = Convert.ToInt32(row.Cells["dgvQty"].Value);

                    // Ensure the quantity is greater than 0 before decrementing
                    if (currentQty > 0)
                    {
                        row.Cells["dgvQty"].Value = currentQty - 1; // Decrement the quantity by 1
                        double price = Convert.ToDouble(row.Cells["dgvPrice"].Value);
                        row.Cells["dgvAmount"].Value = (currentQty - 1) * price; // Update the amount
                    }
                    else
                    {
                        rowsToRemove.Add(row); // Add the row to the list of rows to be removed
                    }
                }
            }

            // Remove the selected rows that have a quantity of 0
            foreach (DataGridViewRow rowToRemove in rowsToRemove)
            {
                guna2DataGridView1.Rows.Remove(rowToRemove);
            }

            // Update the total amount
            GetTotal();
        }

        private void txtdeliverycharges_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed key is not a digit and not a control key (e.g., backspace, delete)
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                // Cancel the key press event (prevent the character from being entered)
                e.Handled = true;
            }
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
    }
}
