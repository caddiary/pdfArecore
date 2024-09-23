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
	public partial class Form6 : Form
	{
		public int m_pageSel = -1;
		public bool m_isFile = true;

		public Form6(int page_first, int page_max)
		{
			InitializeComponent();

			for (int i = 1; i <= page_max; i++)
				comboBox1.Items.Add(i.ToString());
			comboBox1.Items.Add("最終ページ");
			comboBox1.SelectedIndex = page_first - 1;
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			m_isFile = true;
		}
		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			m_isFile = false;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string str = comboBox1.SelectedItem.ToString();
			if (!int.TryParse(str, out m_pageSel))
			{
				if (str == "最終ページ")
					m_pageSel = 0;
			}
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
