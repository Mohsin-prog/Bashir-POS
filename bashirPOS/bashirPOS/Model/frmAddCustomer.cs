using System;
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
    public partial class frmAddCustomer : Form
    {
        public frmAddCustomer()
        {
            InitializeComponent();
        }
        public string orderType = "";
        public int mainID = 0;
        private void frmAddCustomer_Load(object sender, EventArgs e)
        {
            if (orderType == "TakeAway")
            {
                // Customize the form for TakeAway orders
                this.Text = "TakeAway Customer Information"; // Change the form title
                txtName.Text = "Customer Name (Optional):"; // Change the label text for name
                txtPhone.Text = "Customer Phone (Optional):"; // Change the label text for phone

            }

        }
    }
}
