using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace bashirPOS
{
    class MainClass
    {

        //public static string con_string = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=" + System.IO.Path.GetFullPath("BashirHistory.mdf") + ";Integrated Security=True;";
        //public static SqlConnection con = new SqlConnection(con_string);


       // public static string con_string = @"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=" + System.IO.Path.GetFullPath("BashirHistory.mdf") + ";Integrated Security=True;";

        //public static string con_string = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={AppDomain.CurrentDomain.BaseDirectory}BashirHistory.mdf;Integrated Security=True";

        //public static SqlConnection con = new SqlConnection(con_string);

        //public static string con_string = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BashirHistory.mdf;Integrated Security=True";

        public static string con_string = @"Data Source=(localdb)\MSSQLLocalDB;Database=BashirHistory;Integrated Security=True";

        //public static string con_string = "Data Source=DESKTOP-KD3V6G8\\SQLEXPRESS01;Initial Catalog=BashirHistory;Integrated Security=True;";

        public static SqlConnection con = new SqlConnection(con_string);



        //Method for CRUD Operation


        public static int SQl(string qry, Hashtable ht)
        {
            int res = 0;

            try
            {
                SqlCommand cmd = new SqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;

                // Add parameters using parameterized queries
                foreach (DictionaryEntry item in ht)
                {
                    cmd.Parameters.AddWithValue(item.Key.ToString(), item.Value);
                }

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                res = cmd.ExecuteNonQuery();

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                con.Close();
            }
            return res;
        }




        //For Loading Data from database


        public static void LoadData(string qry, DataGridView gv, ListBox lb)
        {
            //Serial Number in GridView

            gv.CellFormatting += new DataGridViewCellFormattingEventHandler(gv_CellFormatting);
            try
            {
                SqlCommand cmd = new SqlCommand(qry, con);
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < lb.Items.Count; i++)
                {
                    string colNam1 = ((DataGridViewColumn)lb.Items[i]).Name;
                    gv.Columns[colNam1].DataPropertyName = dt.Columns[i].ToString();
                }

                gv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                con.Close();
            }

        }
        private static void gv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            Guna.UI2.WinForms.Guna2DataGridView gv = (Guna.UI2.WinForms.Guna2DataGridView)sender;
            int count = 0;

            foreach (DataGridViewRow row in gv.Rows)
            {
                count++;
                row.Cells[0].Value = count;

            }
        }

        public static void BlurBackground(Form Model)
        {
            Form Background = new Form();
            using (Model)
            {
                Background.StartPosition = FormStartPosition.Manual;
                Background.FormBorderStyle = FormBorderStyle.None;
                Background.Opacity = 0.5d;
                Background.BackColor = Color.Black;
                Background.Size = Form1.Intance.Size;
                Background.Location = Form1.Intance.Location;
                Background.ShowInTaskbar = false;
                Background.Show();
                Model.Owner = Background;
                Model.ShowDialog(Background);
                Background.Dispose();
            }
        }
        //For CB Fill 

        public static void CBFILL(string qry, ComboBox cb)
        {
            SqlCommand cmd = new SqlCommand(qry, con);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cb.DisplayMember = "name";
            cb.ValueMember = "id";
            cb.DataSource = dt;
            cb.SelectedIndex = -1;
        }

        private static object ExecuteScalar(SqlCommand cmd)
        {
            object result = null;

            using (SqlConnection connection = new SqlConnection(con_string))
            {
                try
                {
                    connection.Open();
                    cmd.Connection = connection;
                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            return result;
        }

        public static object Scalar(string qry, Hashtable ht)
        {
            object result = null;

            try
            {
                using (SqlCommand cmd = new SqlCommand(qry))
                {
                    // Add parameters using parameterized queries
                    foreach (DictionaryEntry item in ht)
                    {
                        cmd.Parameters.AddWithValue(item.Key.ToString(), item.Value);
                    }

                    result = ExecuteScalar(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return result;
        }
    }
}




