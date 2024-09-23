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
	public partial class Form5 : Form
	{
		public int m_pageFrom = -1;
		public int m_pageTo = -1;
		private int m_pageMax = -1;
		public bool m_blPageDel = false;

		public Form5(int page_now, int page_max)
		{
			InitializeComponent();
			m_pageMax = page_max;
			textBox1.Text = page_now.ToString();
			textBox2.Text = page_now.ToString();
			label3.Text = "/" + page_max.ToString();
			checkBox1.Checked = m_blPageDel;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			m_pageFrom = int.Parse(textBox1.Text);
			m_pageTo = int.Parse(textBox2.Text);
			m_blPageDel = checkBox1.Checked;
			if (!(1 <= m_pageFrom && m_pageFrom <= m_pageMax))
			{
				MessageBox.Show("開始ページが異常");
				return;
			}
			if (!(1 <= m_pageTo && m_pageTo <= m_pageMax))
			{
				MessageBox.Show("終了ページが異常");
				return;
			}
			if (m_pageTo < m_pageFrom)
			{
				MessageBox.Show("開始ページと終了ページが逆");
				return;
			}
			if (m_pageFrom == 1 && m_pageTo == m_pageMax)
			{
				MessageBox.Show("全ページは抽出できない");
				return;
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
		}
	}
}
