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
	class FuncImage
	{
		Uty uty = new Uty();
		PdfDocument m_pdfDoc;

		public FuncImage()
		{
		}
		public void Exec(PdfDocument pdfDoc, string strSrc, int type, float kzd, int quarity, int ski, bool isMPTiff)
		{
			m_pdfDoc = pdfDoc;
			// 変換先ファイル名
			int pos = strSrc.LastIndexOf('.');
			string strDst = strSrc.Substring(0, pos) + "[{0}]";
			if (type == 0 || type == 1 || type == 2)
				strDst += ".bmp";
			else if (type == 3)
				strDst += ".jpg";
			else if (type == 4)
				strDst += ".png";
			else if (type == 5 || type == 6)
				strDst += ".tif";
			else
				return;

			Cursor preCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor; // Waitカーソル
			string operation = "画像化";
			bool stat = true;
			string detail = "";
			Stopwatch sw = uty.timeStart();
			try
			{
				if (isMPTiff && (type == 5 || type == 6))
				{
					string output = string.Format(strDst, "All");
					SaveTiffMulti(output, kzd, (type == 5) ? true : false);
				}
				else
				{
					for (int i = 0; i < m_pdfDoc.PageCount; i++)
					{
						SizeF size = m_pdfDoc.PageSizes[i];
						int w = uty.Point2Pixel(size.Width, (double)kzd);
						int h = uty.Point2Pixel(size.Height, (double)kzd);
						Image img = m_pdfDoc.Render(i, w, h, kzd, kzd, false);
						string output = string.Format(strDst, i + 1);
						if (type == 0)  // Bitmap（フルカラー）
							img.Save(output, ImageFormat.Bmp);
						else if (type == 1)  // Bitmap（グレースケール）
							SaveBmpGraycale(img, output);
						else if (type == 2)  // Bitmap（白黒2値）
							SaveBmp1bpp(img, output, ski);
						else if (type == 3)  // Jpeg
							SaveJpeg(img, output, quarity);
						else if (type == 4)  // PNG
							img.Save(output, ImageFormat.Png);
						else if (type == 5)
							SaveTiff(img, output, true);
						else if (type == 6)
							SaveTiff(img, output, false);
						img.Dispose();
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
		// BMPをグレースケールで保存
		private void SaveBmpGraycale(Image img, string output)
		{
			Bitmap bmp = new Bitmap(img);
			BitmapData bmpdata = bmp.LockBits(
				new Rectangle(0, 0, bmp.Width, bmp.Height),
				ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int rp = (int)(0.298912 * 1024);
			int gp = (int)(0.586611 * 1024);
			int bp = (int)(0.114478 * 1024);
			byte[] ba = new byte[bmp.Width * bmp.Height * 4];
			Marshal.Copy(bmpdata.Scan0, ba, 0, ba.Length);
			int pixsize = bmp.Width * bmp.Height * 4;
			for (int i = 0; i < pixsize; i += 4)
			{
				byte g = (byte)((bp * ba[i + 0] + gp * ba[i + 1] + rp * ba[i + 2]) >> 10);
				ba[i + 0] = g;      // ブルー
				ba[i + 1] = g;      // グリーン
				ba[i + 2] = g;      // レッド
				ba[i + 3] = 0xFF;   // アルファ
			}
			Marshal.Copy(ba, 0, bmpdata.Scan0, ba.Length);
			bmp.UnlockBits(bmpdata);
			bmp.Save(output);
		}

		// BMPを白黒2値で保存
		private void SaveBmp1bpp(Image img, string output, int ski)
		{
			Bitmap src = new Bitmap(img);
			Bitmap dest = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
			BitmapData srcBitmapData = src.LockBits(
					new Rectangle(0, 0, src.Width, src.Height),
					ImageLockMode.WriteOnly, src.PixelFormat);
			BitmapData destBitmapData = dest.LockBits(
					new Rectangle(0, 0, dest.Width, dest.Height),
					ImageLockMode.WriteOnly, dest.PixelFormat);

			byte[] srcPixels = new byte[srcBitmapData.Stride * src.Height];
			Marshal.Copy(srcBitmapData.Scan0, srcPixels, 0, srcPixels.Length);

			byte[] destPixels = new byte[destBitmapData.Stride * destBitmapData.Height];
			for (int y = 0; y < destBitmapData.Height; y++)
			{
				for (int x = 0; x < destBitmapData.Width; x++)
				{
					if (ski <= ConvertToGrayscale(srcPixels, x, y, srcBitmapData.Stride))  // 閾値判定
					{
						int pos = (x >> 3) + destBitmapData.Stride * y;  // 位置を決める
						destPixels[pos] |= (byte)(0x80 >> (x & 0x7));  // 白にする
					}
				}
			}
			Marshal.Copy(destPixels, 0, destBitmapData.Scan0, destPixels.Length);
			src.UnlockBits(srcBitmapData);
			dest.UnlockBits(destBitmapData);
			dest.Save(output);
		}

		const int RedFactor = (int)(0.298912 * 1024);
		const int GreenFactor = (int)(0.586611 * 1024);
		const int BlueFactor = (int)(0.114478 * 1024);
		private static float ConvertToGrayscale(byte[] srcPixels, int x, int y, int stride)
		{
			int position = x * 4 + stride * y;  // 3⇒4
			byte b = srcPixels[position + 0];
			byte g = srcPixels[position + 1];
			byte r = srcPixels[position + 2];

			return (r * RedFactor + g * GreenFactor + b * BlueFactor) >> 10;
		}

		// Jpegに保存
		private void SaveJpeg(Image img, string output, Int64 quality)
		{
			ImageCodecInfo jpgEncoder = null;
			foreach (ImageCodecInfo ici in ImageCodecInfo.GetImageEncoders())
			{
				if (ici.FormatID == ImageFormat.Jpeg.Guid)
				{
					jpgEncoder = ici;
					break;
				}
			}
			if (jpgEncoder != null)
			{
				EncoderParameter encParam = new EncoderParameter(Encoder.Quality, quality);
				EncoderParameters encParams = new EncoderParameters(1);
				encParams.Param[0] = encParam;
				img.Save(output, jpgEncoder, encParams);
			}
		}

		// Tiffに保存
		private void SaveTiff(Image img, string output, bool isFullColor)
		{
			TiffBitmapEncoder encoder = new TiffBitmapEncoder();
			if (isFullColor)
				encoder.Compression = TiffCompressOption.Lzw;
			else
				encoder.Compression = TiffCompressOption.Ccitt4;
			MemoryStream ms = new MemoryStream();
			img.Save(ms, ImageFormat.Bmp);
			BitmapFrame bmpFrame = BitmapFrame.Create(ms);
			encoder.Frames.Add(bmpFrame);
			using (FileStream outputFileStrm = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				encoder.Save(outputFileStrm);
				outputFileStrm.Close();
			}
		}

		// Tiffマルチページに保存
		private void SaveTiffMulti(string output, float kzd, bool isFullColor)
		{
			TiffBitmapEncoder encoder = new TiffBitmapEncoder();
			if (isFullColor)
				encoder.Compression = TiffCompressOption.Lzw;
			else
				encoder.Compression = TiffCompressOption.Ccitt4;
			for (int i = 0; i < m_pdfDoc.PageCount; i++)
			{
				SizeF size = m_pdfDoc.PageSizes[i];
				int w = uty.Point2Pixel(size.Width, (double)kzd);
				int h = uty.Point2Pixel(size.Height, (double)kzd);
				Image img = m_pdfDoc.Render(i, w, h, kzd, kzd, false);
				MemoryStream ms = new MemoryStream();
				img.Save(ms, ImageFormat.Bmp);
				BitmapFrame bmpFrame = BitmapFrame.Create(ms);
				encoder.Frames.Add(bmpFrame);
			}
			using (FileStream outputFileStrm = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				encoder.Save(outputFileStrm);
				outputFileStrm.Close();
			}
		}
	}
}
