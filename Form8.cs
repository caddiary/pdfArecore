using PdfiumViewer;
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
	public partial class Form8 : Form
	{
		ITSharp its = new ITSharp();
		Uty uty = new Uty();

		public PdfDocument m_pdfDoc;
		public string m_pdfName;
		public string m_tempFile;
		public string m_pdfPsw;

		public Form8(PdfDocument pdfDoc)
		{
			InitializeComponent();
			m_pdfDoc = pdfDoc;
			checkBox1.Enabled = false;
			checkBox2.Enabled = false;
			checkBox3.Enabled = false;
			checkBox4.Enabled = false;
			checkBox5.Enabled = false;
			checkBox6.Enabled = false;
			checkBox7.Enabled = false;
			checkBox8.Enabled = false;

			checkBox1.Checked = true;
			checkBox3.Checked = true;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string pw1 = textBox1.Text;
			string pw2 = textBox2.Text;
			if (string.IsNullOrEmpty(pw1) && string.IsNullOrEmpty(pw2))
				return;
			if (pw1 == pw2)
			{
				MessageBox.Show("読取パスワードと編集パスワードを同一にすることはできない");
				return;
			}
			bool[] aryCheck = new bool[8];
			aryCheck[0] = checkBox1.Checked;
			aryCheck[1] = checkBox2.Checked;
			aryCheck[2] = checkBox3.Checked;
			aryCheck[3] = checkBox4.Checked;
			aryCheck[4] = checkBox5.Checked;
			aryCheck[5] = checkBox6.Checked;
			aryCheck[6] = checkBox7.Checked;
			aryCheck[7] = checkBox8.Checked;

			// 一度保存してファイルにする
			if (string.IsNullOrWhiteSpace(m_tempFile))
			{
				m_tempFile = uty.GetTempPDFName();
				m_pdfDoc.Save(m_tempFile);
			}
			m_pdfDoc.Dispose();
			if (its.SetSecurity(ref m_tempFile, pw1, pw2, aryCheck))
			{
				try
				{
					string pw = string.IsNullOrEmpty(pw1) ? pw2 : pw1;
					m_pdfDoc = PdfDocument.Load(m_tempFile, pw);
					m_pdfName = m_tempFile;
					m_pdfPsw = pw;
				}
				catch (PdfException ex)
				{
					MessageBox.Show(string.Format("pdfファイルオープンに失敗（{0}）", ex.Message));
				}
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			bool enable = string.IsNullOrEmpty(textBox2.Text) ? false : true;
			checkBox1.Enabled = enable;
			checkBox2.Enabled = enable;
			checkBox3.Enabled = enable;
			checkBox4.Enabled = enable;
			checkBox5.Enabled = enable;
			checkBox6.Enabled = enable;
			checkBox7.Enabled = enable;
			checkBox8.Enabled = enable;
		}
	}
}
