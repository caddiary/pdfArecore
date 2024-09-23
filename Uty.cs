using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace PDFiumTest
{
	class Uty
	{
		public Uty()
		{
		}
		public string GetTempPDFName()
		{
			string temp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
			temp += ".pdf";
			return temp;
		}
		public bool FileDelete(string file)
		{
			const int RETRY_CNT = 300;
			bool stat = true;
			for (int i = 0; i < RETRY_CNT; i++)  // トータル30秒リトライする
			{
				try
				{
					stat = true;
					File.Delete(file);
				}
				catch
				{
					stat = false;
					Thread.Sleep(100);  // 少し待ってリトライ
				}
				if (stat)
					break;
			}
			if (!stat)
				MessageBox.Show("ファイル削除に失敗");
			return stat;
		}
		public double mm2point(double mm)
		{
			return mm * 72.0 / 25.4;
		}
		public double point2mm(double pnt)
		{
			return pnt / 72.0 * 25.4;
		}
		public int Point2Pixel(double pnt, double kzd)
		{
			double inch = pnt / 72.0;
			double px = kzd * inch;
			return (int)Math.Ceiling(px);
		}

		// 時間計測開始＆終了
		public Stopwatch timeStart()
		{
			Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			return sw;
		}

		public double timeEnd(Stopwatch sw)
		{
			sw.Stop();
			TimeSpan ts = sw.Elapsed;
			double time = (double)ts.TotalMilliseconds / 1000.0;
			time = Math.Round(time, 2, MidpointRounding.AwayFromZero);
			return time;
		}

		// ログ追加
		public void addLog(string operation, bool stat, double time, string detail)
		{
			string logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			logFile = Path.Combine(logFile, "caddiary");
			SafeCreateDirectory(logFile);
			logFile = Path.Combine(logFile, "pdfArecore");
			SafeCreateDirectory(logFile);
			logFile = Path.Combine(logFile, "log.txt");
			StreamWriter sw = new StreamWriter(logFile, true, Encoding.GetEncoding("Shift_JIS"));
			string line = operation + "\t";
			line += time.ToString() + "\t";
			line += stat ? "成功" : "失敗" + "\t";
			line += detail;
			sw.WriteLine(line);
			sw.Dispose();
		}
		public void SafeCreateDirectory(string path)
		{
			if (Directory.Exists(path))
				return;
			Directory.CreateDirectory(path);
		}
	}
}
