using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace PDFiumTest
{
	class ITSharp
	{
		Uty uty = new Uty();

		public ITSharp()
		{
		}
		public bool SetInformation(ref string file, string[] aryStr)
		{
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader reader = new PdfReader(file);
			try
			{
				PdfStamper stamper = new PdfStamper(reader, new FileStream(temp, FileMode.Create));
				Dictionary<String, String> info = reader.Info;
				info["Title"] = aryStr[0];
				info["Author"] = aryStr[1];
				info["Subject"] = aryStr[2];
				info["Keywords"] = aryStr[3];
				info["Creator"] = aryStr[4];
				stamper.MoreInfo = info;
				//stamper.FormFlattening = true;
				stamper.Close();
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("プロパティ設定に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}
		public bool SetSecurity(ref string file, string pw1, string pw2, bool[] aryCheck)
		{
			byte[] byteUSER = Encoding.ASCII.GetBytes(pw1);
			byte[] byteOWNER = Encoding.ASCII.GetBytes(pw2);
			int permissionAll = PdfWriter.AllowPrinting | PdfWriter.AllowModifyContents | PdfWriter.AllowCopy |
				PdfWriter.AllowModifyAnnotations | PdfWriter.AllowFillIn | PdfWriter.AllowScreenReaders |
				PdfWriter.AllowAssembly | PdfWriter.AllowDegradedPrinting;
			int Length = PdfWriter.ENCRYPTION_AES_256 | PdfWriter.DO_NOT_ENCRYPT_METADATA;
			int permission = 0;
			if (aryCheck[0])
				permission |= PdfWriter.AllowPrinting;  // 印刷
			if (aryCheck[1])
				permission |= PdfWriter.AllowModifyContents;  // 文書の変更
			if (aryCheck[2])
				permission |= PdfWriter.AllowCopy;  // 内容のコピーと抽出
			if (aryCheck[3])
				permission |= PdfWriter.AllowModifyAnnotations;  //注釈
			if (aryCheck[4])
				permission |= PdfWriter.AllowFillIn;  // フォームフィールドの入力と署名
			if (aryCheck[5])
				permission |= PdfWriter.AllowScreenReaders;  // アクセシビリティ
			if (aryCheck[6])
				permission |= PdfWriter.AllowAssembly;  // 文章アセンブリ
			if (aryCheck[7])
				permission |= PdfWriter.AllowDegradedPrinting;  // 低解像度印刷

			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader reader = new PdfReader(file);
			try
			{
				PdfStamper stamper = new PdfStamper(reader, new FileStream(temp, FileMode.Create));
				if (string.IsNullOrEmpty(pw2))  // 読取のみ
					stamper.SetEncryption(byteUSER, byteUSER, permissionAll, Length);
				else if (string.IsNullOrEmpty(pw1))  // 編集のみ
					stamper.SetEncryption(null, byteOWNER, permission, Length);
				else  // 読取と編集
					stamper.SetEncryption(byteUSER, byteOWNER, permission, Length);
				stamper.Close();
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("セキュリティ設定に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}

		private void _pageDel(PdfReader reader, string temp, bool blAll, bool blDel, params int[] pages)
		{
			using (FileStream fileStream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))  // 削除後PDFを作成する
			{
				using (Document document = new Document())
				{
					using (PdfWriter pdfWriter = PdfWriter.GetInstance(document, fileStream))
					{
						document.Open();  // 削除後PDFを開く
						for (int i = 1; i <= reader.NumberOfPages; i++)
						{
							bool judge = pages.Contains(i);
							if (!blAll)  // trueなら全ページを対象とする（抽出で元ファイルから削除しないときの動作）
							{
								if (blDel)  // trueなら指定ページ以外を抜き出す、falseなら指定ページを抜き出す
									judge = !judge;
							}
							else
								judge = true;
							if (judge)
							{
								int rotate = reader.GetPageRotation(i);
								float height = reader.GetPageSizeWithRotation(i).Height;
								float width = reader.GetPageSizeWithRotation(i).Width;
								document.SetPageSize(reader.GetPageSizeWithRotation(i));
								document.NewPage();
								PdfImportedPage page = pdfWriter.GetImportedPage(reader, i);
								if (rotate == 0)
									pdfWriter.DirectContent.AddTemplate(page, 1, 0, 0, 1, 0, 0);
								else if (rotate == 90)
									pdfWriter.DirectContent.AddTemplate(page, 0, -1, 1, 0, 0, height);
								else if (rotate == 180)
									pdfWriter.DirectContent.AddTemplate(page, -1, 0, 0, -1, width, height);
								else // if (rotate == 270)
									pdfWriter.DirectContent.AddTemplate(page, 0, 1, -1, 0, width, 0);
							}
						}
						document.Close();
					}
				}
			}
		}

		public bool PageDelete(ref string file, params int[] pages)
		{
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader reader = new PdfReader(file);
			try
			{
				_pageDel(reader, temp, false, true, pages);
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("ページ削除に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}

		public bool PageExtract(ref string file, bool blDel, string savepath, params int[] pages)
		{
			bool blNoEdit = false;
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader reader = new PdfReader(file);
			try
			{
				// 抽出
				_pageDel(reader, savepath, false, false, pages);
				// 元ファイルからの削除（削除しない場合も更新している）
				_pageDel(reader, temp, blDel ?false :true, true, pages);
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("ページ削除に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader.Close();
				if (stat)
				{
					if (blNoEdit)  // 元ファイルは何もしないので、falseで返して更新しない
						stat = false;
					else
					{
						uty.FileDelete(file);
						file = temp;
					}
				}
			}
			return stat;
		}

		private bool emptyPage(iTextSharp.text.Rectangle size, out string strEmp)
		{
			bool stat = true;
			strEmp = null;
			try
			{
				using (Document docEmp = new Document(size))
				{
					using (MemoryStream memEmp = new MemoryStream())
					{
						using (PdfWriter writer = PdfWriter.GetInstance(docEmp, memEmp))
						{
							docEmp.Open();
							PdfContentByte pdfContentByte = writer.DirectContent;
							FontFactory.RegisterDirectories();
							var baseFont = FontFactory.GetFont("Arial", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED, 8f);
							pdfContentByte.BeginText();
							pdfContentByte.SetFontAndSize(baseFont.BaseFont, 8);
							pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, " ", 100f, 100f, 0);
							pdfContentByte.EndText();
							docEmp.Close();
							strEmp = uty.GetTempPDFName();
							using (BinaryWriter w = new BinaryWriter(File.OpenWrite(strEmp)))
							{
								w.Write(memEmp.ToArray());
							}
						}
					}
				}
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("空ページ作成に失敗（{0}）", ex.Message));
			}
			finally
			{
				if (!stat && strEmp != null)
					uty.FileDelete(strEmp);
			}
			return stat;
		}

		public int PageInsert(ref string file, int selpage, string inspath, int page)
		{
			string temp = uty.GetTempPDFName();
			int stat = 1;  // 挿入したページの最後のページ
			PdfReader reader1 = new PdfReader(file);
			PdfReader reader2 = null;
			PdfReader reader3 = null;
			string emptyFile = null;
			if (inspath != null)
				reader2 = new PdfReader(inspath);
			else
			{
				if (emptyPage(reader1.GetPageSizeWithRotation(page), out emptyFile))
					reader3 = new PdfReader(emptyFile);
				else
					return 0;
			}
			try
			{
				using (Document document = new Document())
				{
					using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						using (PdfCopy copy = new PdfCopy(document, stream))
						{
							document.Open();
							if (selpage == 1)  // 先頭に挿入
							{
								if (reader2 != null)
								{
									for (int i = 1; i <= reader2.NumberOfPages; i++)
										copy.AddPage(copy.GetImportedPage(reader2, i));
									stat = reader2.NumberOfPages;
								}
								else
								{
									copy.AddPage(copy.GetImportedPage(reader3, 1));
									stat = 1;
								}
								for (int i = 1; i <= reader1.NumberOfPages; i++)
									copy.AddPage(copy.GetImportedPage(reader1, i));
							}
							else if (2 <= selpage && selpage <= reader1.NumberOfPages)  // 途中に挿入
							{
								for (int i = 1; i < selpage; i++)
									copy.AddPage(copy.GetImportedPage(reader1, i));
								if (reader2 != null)
								{
									for (int i = 1; i <= reader2.NumberOfPages; i++)
										copy.AddPage(copy.GetImportedPage(reader2, i));
									stat = selpage - 1 + reader2.NumberOfPages;
								}
								else
								{
									copy.AddPage(copy.GetImportedPage(reader3, 1));
									stat = selpage;
								}
								for (int i = selpage; i <= reader1.NumberOfPages; i++)
									copy.AddPage(copy.GetImportedPage(reader1, i));
							}
							else if (selpage == 0)  // 最後に追加
							{
								for (int i = 1; i <= reader1.NumberOfPages; i++)
									copy.AddPage(copy.GetImportedPage(reader1, i));
								if (reader2 != null)
								{
									for (int i = 1; i <= reader2.NumberOfPages; i++)
										copy.AddPage(copy.GetImportedPage(reader2, i));
									stat = reader1.NumberOfPages + reader2.NumberOfPages;
								}
								else
								{
									copy.AddPage(copy.GetImportedPage(reader3, 1));
									stat = reader1.NumberOfPages + 1;
								}
							}
						}
					}
					document.Close();
				}
			}
			catch (PdfException ex)
			{
				stat = 0;
				MessageBox.Show(string.Format("ページ挿入に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader1.Close();
				if (reader2 != null)
					reader2.Close();
				if (reader3 != null)
				{
					reader3.Close();
					uty.FileDelete(emptyFile);
				}
				if (stat != 0)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}

		public bool PageRotate(ref string file, int pno, int degree, string pdfPsw)
		// degree: 90 or 180 or 270
		{
			byte[] bytePsw = Encoding.ASCII.GetBytes(pdfPsw);
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader reader;
			if (string.IsNullOrEmpty(pdfPsw))
				reader = new PdfReader(file);
			else
				reader = new PdfReader(file, bytePsw);
//			PdfReader reader = new PdfReader(file, bytePsw);
			try
			{
				int pagesCount = reader.NumberOfPages;
				if (0 < pno && pno <= pagesCount)
				{
					PdfDictionary page = reader.GetPageN(pno);
					PdfNumber rotate = page.GetAsNumber(PdfName.ROTATE);
					int rotation = rotate == null ? degree : (rotate.IntValue + degree) % 360;
					page.Put(PdfName.ROTATE, new PdfNumber(rotation));

					PdfStamper stamper = new PdfStamper(reader, new FileStream(temp, FileMode.Create));
					stamper.Close();
				}
				else
				{
					stat = false;
					MessageBox.Show("ページが範囲外");
				}
			}
			catch(PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("ページ回転に失敗（{0}）", ex.Message));
			}
			finally
			{
				reader.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}

		public bool AddNatsuin(ref bool isFirst, ref string file, int pno, string strNtin, double rx, double ry, double nsize)
		{
			bool stat = false;
			if (AddEmptyImg(ref isFirst, ref file, pno, rx, ry, nsize))  // まずは空の画像を配置
			{
				string ext = Path.GetExtension(strNtin);
				if (string.Compare(ext, ".pdf", true) == 0)
					stat = AddPdf(ref file, pno, strNtin, rx, ry);
				else if (string.Compare(ext, ".png", true) == 0)
					stat = AddImg(ref file, pno, strNtin, rx, ry, nsize);
			}
			return stat;
		}

		public bool AddPdf(ref string file, int pno, string strNtin, double rx, double ry)
		{
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader src = new PdfReader(file);
			PdfReader add = new PdfReader(strNtin);
			try
			{
				int pagesCount = src.NumberOfPages;
				if (0 < pno && pno <= pagesCount)
				{
					iTextSharp.text.Rectangle srcBox = src.GetPageSize(pno);
					iTextSharp.text.Rectangle addBox = add.GetPageSize(1);
					int rot = src.GetPageRotation(pno);
					double pos_y = 0, pos_x = 0;
					if (rot == 0 || rot == 180)
					{
						pos_y = srcBox.Height * ry;
						pos_x = srcBox.Width * rx;
					}
					else  // 90度または270度回転していると、高さ幅が逆になる
					{
						pos_y = srcBox.Width * ry;
						pos_x = srcBox.Height * rx;
					}
					pos_y -= addBox.Height / 2.0;  // 捺印用PDFは中央に捺印イメージがある前提なので、用紙サイズから半分引く
					pos_x -= addBox.Width / 2.0;
					PdfStamper stamper = new PdfStamper(src, new FileStream(temp, FileMode.Create));
					PdfImportedPage page = stamper.GetImportedPage(add, 1);
					stamper.GetImportedPage(src, pno);
					PdfContentByte cb = stamper.GetOverContent(pno);
					cb.AddTemplate(page, pos_x, pos_y);  // 配置点はここで指定する
					stamper.Close();
				}
				else
				{
					stat = false;
					MessageBox.Show("ページが範囲外");
				}
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("捺印に失敗（{0}）", ex.Message));
			}
			finally
			{
				src.Close();
				add.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}

		public bool AddEmptyImg(ref bool isFirst, ref string file, int pno, double rx, double ry, double nsize)
		{
			if (isFirst == false)
			{
				isFirst = true;
				string strImg = Path.GetDirectoryName(Application.ExecutablePath);
				strImg = Path.Combine(strImg, "empty.png");
				return AddImg(ref file, pno, strImg, rx, ry, nsize);
			}
			return true;
		}

		public bool AddImg(ref string file, int pno, string strImg, double rx, double ry, double nsize)
		{
			string temp = uty.GetTempPDFName();
			bool stat = true;
			PdfReader src = new PdfReader(file);
			try
			{
				int pagesCount = src.NumberOfPages;
				if (0 < pno && pno <= pagesCount)
				{
					iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(strImg);
					iTextSharp.text.Rectangle srcBox = src.GetPageSize(pno);
					int rot = src.GetPageRotation(pno);
					double pos_y = 0, pos_x = 0;
					if (rot == 0 || rot == 180)
					{
						pos_y = srcBox.Height * ry;
						pos_x = srcBox.Width * rx;
					}
					else  // 90度または270度回転していると、高さ幅が逆になる
					{
						pos_y = srcBox.Width * ry;
						pos_x = srcBox.Height * rx;
					}
					double imgSizePoint = uty.mm2point(nsize);
					pos_y -= imgSizePoint / 2.0;  // 捺印用PDFは中央に捺印イメージがある前提なので、用紙サイズから半分引く
					pos_x -= imgSizePoint / 2.0;
					PdfStamper stamper = new PdfStamper(src, new FileStream(temp, FileMode.Create));
					PdfContentByte cb = stamper.GetOverContent(pno);
					image.ScaleAbsoluteHeight((float)imgSizePoint);
					image.ScaleAbsoluteWidth((float)imgSizePoint);
					image.SetAbsolutePosition((float)pos_x, (float)pos_y);
					cb.AddImage(image);
					stamper.Close();
				}
				else
				{
					stat = false;
					MessageBox.Show("ページが範囲外");
				}
			}
			catch (PdfException ex)
			{
				stat = false;
				MessageBox.Show(string.Format("捺印に失敗（{0}）", ex.Message));
			}
			finally
			{
				src.Close();
				if (stat)
				{
					uty.FileDelete(file);
					file = temp;
				}
			}
			return stat;
		}
	}
}
