using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFiumTest
{
	public partial class Form3 : Form
	{
		public string password { get; set; }

		public Form3()
		{
			InitializeComponent();
		}
		private void button1_Click(object sender, EventArgs e)
		{
			password = textBox1.Text;
			DialogResult = DialogResult.OK;
			Close();
		}
		private void button2_Click(object sender, EventArgs e)
		{
		}
	}
}
