using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bashirPOS.Model
{
    public partial class ucProduct : UserControl
    {

        public event EventHandler onSelect = null; 
        public ucProduct()
        {
            InitializeComponent();
        }

        public int id { get; set;}

        
        public string PCategory { get; set; }
        public string PName
        {
            get { return lblName.Text; }
            set { lblName.Text = value; }
        }
        public string PPrice
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public Image PImage
        {
            get { return txtImage.Image; }
            set { txtImage.Image = value; }
        }

        private void txtImage_Click(object sender, EventArgs e)
        {
            onSelect?.Invoke(this, e);
        }
    }
}
