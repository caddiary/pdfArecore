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
using System.IO;
using System.Diagnostics;
using Encoder = System.Drawing.Imaging.Encoder;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PDFiumTest
{
	public partial class Form7 : Form
	{
		Uty uty = new Uty();
		FuncImage fcImage = new FuncImage();
		string m_strSrc;
		PdfDocument m_pdfDoc;
		Option m_opt;

		public Form7(string strSrc, PdfDocument pdfDoc, Option opt)
		{
			InitializeComponent();
			m_strSrc = strSrc;
			m_pdfDoc = pdfDoc;
			m_opt = opt;

			comboBox2.Text = opt.m_ImageKzd.ToString();
			comboBox1.SelectedIndex = opt.m_ImageType;
			textBox4.Text = opt.m_ImageJpegQ.ToString();
			if (opt.m_ImageType != 3)
				textBox4.ReadOnly = true;
			textBox6.Text = opt.m_ImageBmp2V.ToString();
			if (opt.m_ImageType != 2)
				textBox6.ReadOnly = true;
			checkBox1.Checked = (opt.m_ImageTiffMulti == 1) ? true : false;
			if (!(opt.m_ImageType == 5 || opt.m_ImageType == 6))
				checkBox1.Enabled = false;
		}

		// コントロールチェック
		private bool CheckCntrol(ref int type, ref float kzd, ref int quarity, ref int ski, ref bool isMPTiff)
		{
			type = comboBox1.SelectedIndex;
			string strKzd = comboBox2.Text;
			string strQuality = textBox4.Text;
			string strSki = textBox6.Text;
			isMPTiff = checkBox1.Checked;

			int pos = m_strSrc.LastIndexOf('.');
			string strExt = m_strSrc.Substring(pos + 1);
			if (strExt.ToLower() != "pdf")
			{
				MessageBox.Show("PDFファイルではない");
				return false;
			}
			if (!File.Exists(m_strSrc))
			{
				MessageBox.Show("PDFファイルが存在しない");
				return false;
			}
			if (strKzd == "")
			{
				MessageBox.Show("解像度が空");
				return false;
			}
			if (!float.TryParse(strKzd, out kzd))
			{
				MessageBox.Show("解像度が数値でない");
				return false;
			}
			if (type == 2)  // BMP(白黒2値)の場合だけ
			{
				if (strSki == "")
				{
					MessageBox.Show("BMP白黒判定しきい値が空");
					return false;
				}
				if (!int.TryParse(strSki, out ski))
				{
					MessageBox.Show("BMP白黒判定しきい値が数値でない");
					return false;
				}
				if (!(0 <= ski && ski <= 255))
				{
					MessageBox.Show("BMP白黒判定しきい値が0～255でない");
					return false;
				}
			}
			if (type == 3)  // Jpegの場合だけ
			{
				if (strQuality == "")
				{
					MessageBox.Show("JPEG品質が空");
					return false;
				}
				if (!int.TryParse(strQuality, out quarity))
				{
					MessageBox.Show("JPEG品質が数値でない");
					return false;
				}
				if (!(0 <= quarity && quarity <= 100))
				{
					MessageBox.Show("JPEG品質が0～100でない");
					return false;
				}
			}
			return true;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			// 入力コントロールチェック
			int type = 0, quality = 0, ski = 0;
			float kzd = 0;
			bool isMPTiff = false;
			if (!CheckCntrol(ref type, ref kzd, ref quality, ref ski, ref isMPTiff))
				return;
			fcImage.Exec(m_pdfDoc, m_strSrc, type, kzd, quality, ski, isMPTiff);
			DialogResult = DialogResult.OK;
			Close();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			textBox4.ReadOnly = (comboBox1.SelectedIndex != 3);
			textBox6.ReadOnly = (comboBox1.SelectedIndex != 2);
			checkBox1.Enabled = (comboBox1.SelectedIndex == 5 || comboBox1.SelectedIndex == 6);
		}

		private void Form7_FormClosing(object sender, FormClosingEventArgs e)
		{
			m_opt.m_ImageType = comboBox1.SelectedIndex;
			int.TryParse(comboBox2.Text, out m_opt.m_ImageKzd);
			int.TryParse(textBox4.Text, out m_opt.m_ImageJpegQ);
			int.TryParse(textBox6.Text, out m_opt.m_ImageBmp2V);
			m_opt.m_ImageTiffMulti = (checkBox1.Checked == true) ? 1 : 0;
			m_opt.SaveReg();
		}
	}
}
