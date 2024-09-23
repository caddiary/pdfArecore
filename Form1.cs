using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Markup;

namespace PDFiumTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		///////////////////////////////////////////////////////////////////
		// ファイル選択によるファイルオープン
		private void button1_Click(object sender, EventArgs e)
		{
		}

		// ドラッグ＆ドロップによるファイルオープン
		private void Form1_DragDrop(object sender, DragEventArgs e)
		{
		}
		private void Form1_DragEnter(object sender, DragEventArgs e)
		{
		}

		///////////////////////////////////////////////////////////////////
		// 画像変換
		private void button2_Click(object sender, EventArgs e)
		{
		}

		// 画像形式が変わった
		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		///////////////////////////////////////////////////////////////////
		// テキスト抽出
		private void button3_Click(object sender, EventArgs e)
		{
		}

		///////////////////////////////////////////////////////////////////
		// 描画
		private void button4_Click(object sender, EventArgs e)
		{
		}

		// メインフォームクローズ
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
		}

		// 情報
		private void button6_Click(object sender, EventArgs e)
		{
		}

		//　セーブ
		private void button5_Click(object sender, EventArgs e)
		{
		}

		private void Form1_Load(object sender, EventArgs e)
		{
		}
	}
}

