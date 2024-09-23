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
	public partial class Form4 : Form
	{
		ITSharp its = new ITSharp();
		Uty uty = new Uty();

		public PdfDocument m_pdfDoc;
		public string m_pdfName;
		public string m_tempFile;
		public string m_pdfPsw;

		public Form4(PdfDocument pdfDoc, string pdfPsw)
		{
			InitializeComponent();
			m_pdfDoc = pdfDoc;
			m_pdfPsw = pdfPsw;

			PdfInformation info = pdfDoc.GetInformation();
			textBox1.Text = info.Title;
			textBox2.Text = info.Author;
			textBox3.Text = info.Subject;
			textBox4.Text = info.Keywords;
			DateTime? creationdate = info.CreationDate;
			textBox5.Text = creationdate.ToString();
			DateTime? modificationdate = info.ModificationDate;
			textBox6.Text = modificationdate.ToString();
			textBox7.Text = info.Creator;
			textBox8.Text = info.Producer;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string[] aryStr = new string[5];
			aryStr[0] = textBox1.Text;
			aryStr[1] = textBox2.Text;
			aryStr[2] = textBox3.Text;
			aryStr[3] = textBox4.Text;
			aryStr[4] = textBox7.Text;

			// 一度保存してファイルにする
			if (string.IsNullOrWhiteSpace(m_tempFile))
			{
				m_tempFile = uty.GetTempPDFName();
				m_pdfDoc.Save(m_tempFile);
			}
			m_pdfDoc.Dispose();
			if (its.SetInformation(ref m_tempFile, aryStr))
			{
				try
				{
					if (string.IsNullOrEmpty(m_pdfPsw))
						m_pdfDoc = PdfDocument.Load(m_tempFile);
					else
						m_pdfDoc = PdfDocument.Load(m_tempFile, m_pdfPsw);
					m_pdfName = m_tempFile;
				}
				catch (PdfException ex)
				{
					MessageBox.Show(string.Format("pdfファイルオープンに失敗（{0}）", ex.Message));
				}
			}
			DialogResult = DialogResult.OK;
			Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
		}
	}
}
