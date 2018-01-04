using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacketSniffer
{
    public partial class Form2 : MaterialForm
    {
        public Form2()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Teal600, Primary.Teal800, Primary.Blue200, Accent.Orange700, TextShade.WHITE);
            
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = DataContainer.p.ToString().Replace("]", "\n").Replace("[","");

            if (DataContainer.b == null)
            {
                richTextBox2.Text = "No Data To View";
                return;
            }
            foreach (var b in DataContainer.b)
            {
                richTextBox2.Text += b + " ";
            }

            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
