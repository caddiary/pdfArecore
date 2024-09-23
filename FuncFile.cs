using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFiumTest
{
	class FuncFile
	{
		Uty uty = new Uty();
		public FuncFile()
		{
		}

		public string DlgOpenFile(ref string openpath)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = openpath;
				ofd.Filter = "PDFファイル(*.pdf)|*.pdf";
				ofd.Title = "ロードするPDFファイルを選択";
				ofd.FileName = "";
				ofd.RestoreDirectory = true;
				if (ofd.ShowDialog() != DialogResult.OK)
					return null;
				openpath = Path.GetDirectoryName(ofd.FileName);
				return ofd.FileName;
			}
		}

		public string DlgSaveFile(ref string openpath, string file)
		{
			using (SaveFileDialog sfd = new SaveFileDialog())
			{
				sfd.InitialDirectory = openpath;
				sfd.Filter = "PDFファイル(*.pdf)|*.pdf";
				sfd.Title = "セーブするPDFファイルを選択";
				sfd.FileName = file;
				sfd.OverwritePrompt = true;
				if (sfd.ShowDialog() != DialogResult.OK)
					return null;
				openpath = Path.GetDirectoryName(sfd.FileName);
				return sfd.FileName;
			}
		}

		public string DlgInsertFile(ref string openpath)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = openpath;
				ofd.Filter = "PDFファイル(*.pdf)|*.pdf";
				ofd.Title = "挿入するPDFファイルを選択";
				ofd.FileName = "";
				ofd.RestoreDirectory = true;
				if (ofd.ShowDialog() != DialogResult.OK)
					return null;
				openpath = Path.GetDirectoryName(ofd.FileName);
				return ofd.FileName;
			}
		}

		public string DlgOutputFile(ref string openpath)
		{
			using (SaveFileDialog sfd = new SaveFileDialog())
			{
				sfd.InitialDirectory = openpath;
				sfd.Filter = "PDFファイル(*.pdf)|*.pdf";
				sfd.Title = "抽出結果のPDFファイル名を選択";
				sfd.FileName = "抽出結果";
				sfd.OverwritePrompt = true;
				if (sfd.ShowDialog() != DialogResult.OK)
					return null;
				openpath = Path.GetDirectoryName(sfd.FileName);
				return sfd.FileName;
			}
		}

		// PDFファイルオープン
		public bool pdfOpen(string src, ref PdfDocument pdfDoc, ref string pdfPsw)
		{
			pdfClose(ref pdfDoc);  // すでに開いているかもしれないのでクローズする
			string operation = "ファイルオープン";
			bool stat = false;
			string detail = "";
			Stopwatch sw = uty.timeStart();
			try
			{
				pdfDoc = PdfDocument.Load(src);
			}
			catch (PdfException ex)
			{
				if (ex.Error == PdfError.PasswordProtected)
				{
					if (!string.IsNullOrEmpty(pdfPsw))
						pdfDoc = PdfDocument.Load(src, pdfPsw);
					else
					{
						while (true)
						{
							using (Form3 f = new Form3())
							{
								if (f.ShowDialog() == DialogResult.OK)
								{
									bool stat2 = true;
									try
									{
										pdfDoc = PdfDocument.Load(src, f.password);
									}
									catch (PdfException)
									{
										stat2 = false;
										MessageBox.Show("パスワードが正しくない。");
									}
									if (stat2 && pdfDoc != null && !pdfDoc.Equals(null))
									{
										pdfPsw = f.password;
										break;
									}
								}
								else
								{
									detail = "パスワード入力をキャンセル";
									break;
								}
							}
						}
					}
				}
				else
					detail = string.Format("例外エラー（{0}）", ex.Message);
			}
			finally
			{
				if (pdfDoc != null && !pdfDoc.Equals(null))
					stat = true;
				uty.addLog(operation, stat, uty.timeEnd(sw), detail);
			}
			return stat;
		}

		// PDFファイルクローズ
		public void pdfClose(ref PdfDocument pdfDoc)
		{
			if (pdfDoc == null || pdfDoc.Equals(null))
				return;
			string operation = "ファイルクローズ";
			bool stat = true;
			string detail = "";
			Stopwatch sw = uty.timeStart();
			try
			{
				pdfDoc.Dispose();  // 明示的に破棄する必要あり（PDFがロックされてしまう）
				pdfDoc = null;
			}
			catch (Exception ex)
			{
				stat = false;
				detail = string.Format("例外エラー（{0}）", ex.Message);
			}
			finally
			{
				uty.addLog(operation, stat, uty.timeEnd(sw), detail);
			}
		}

		// PDFファイルセーブ
		public bool pdfSave(string file, ref PdfDocument pdfDoc, string openFile, string pdfPsw)
		{
			string operation = "ファイルセーブ";
			bool stat = true;
			string detail = "";
			Stopwatch sw = uty.timeStart();
			try
			{
				if (string.Compare(file, openFile, true) == 0)  // 上書き保存
				{
					MemoryStream ms = new MemoryStream();
					pdfDoc.Save(ms);
					pdfDoc.Dispose();
					uty.FileDelete(file);
					if (string.IsNullOrEmpty(pdfPsw))
						pdfDoc = PdfDocument.Load(ms);
					else
						pdfDoc = PdfDocument.Load(ms, pdfPsw);
					pdfDoc.Save(file);
					ms.Close();
				}
				else
					pdfDoc.Save(file);
			}
			catch (Exception ex)
			{
				stat = false;
				detail = string.Format("例外エラー（{0}）", ex.Message);
			}
			finally
			{
				uty.addLog(operation, stat, uty.timeEnd(sw), detail);
			}
			return stat;
		}

		// PDFファイルのテキスト出力
		public void TextOut(PdfDocument m_pdfDoc, string strSrc)
		{
			// 変換先ファイル名
			int pos = strSrc.LastIndexOf('.');
			string strDst = strSrc.Substring(0, pos) + ".txt";

			Cursor preCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor; // Waitカーソル
			string operation = "テキスト抽出";
			bool stat = true;
			string detail = "";
			Stopwatch sw = uty.timeStart();
			try
			{
				string text = "";
				for (int i = 0; i < m_pdfDoc.PageCount; i++)
					text += m_pdfDoc.GetPdfText(i);
				if (text != null)
				{
					Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
					using (StreamWriter writer = new StreamWriter(strDst, false, sjisEnc))
					{
						writer.WriteLine(text);
					}
				}
			}
			catch (Exception ex)
			{
				stat = false;
				detail = string.Format("例外エラー（{0}）", ex.Message);
			}
			finally
			{
				uty.addLog(operation, stat, uty.timeEnd(sw), detail);
				Cursor.Current = preCursor;  // Waitカーソル解除
			}
		}
	}
}
