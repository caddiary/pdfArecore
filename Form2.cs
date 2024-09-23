using pdfArecore;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using CredentialManagement;

namespace PDFiumTest
{
	public partial class Form2 : Form
	{
		ITSharp its = new ITSharp();
		Uty uty = new Uty();
		Option m_opt = new Option();
		FuncFile fcFile = new FuncFile();
		FuncImage fcImage = new FuncImage();

		string m_DeleteFile = "";
		string m_OpenFile = "";
		string m_OpenFilePath = "";

		public PdfDocument m_pdfDoc;
		public string m_pdfName;
		public string m_tempFile;
		string m_pdfPsw = "";
		bool m_EditFlag = false;
		bool m_blCtrl = false;
		Cursor m_preCursor;
		Point m_dragStartPos = new Point(0, 0);

		// ページ情報
		PageArray m_pageArray = new PageArray();

		// 捺印関係
		bool m_isFirstNtin = false;
		bool m_isMouseClickMode = false;
		double m_rx = 0;
		double m_ry = 0;
		string m_strNtinFile;
		int m_CursorX = 0;
		int m_CursorY = 0;
		double m_papermm_width = 0;
		double m_papermm_height = 0;

		public Form2()
		{
			this.KeyPreview = true;  // キー入力をフォームで受け取る
			InitializeComponent();
			m_opt.LoadReg();

			pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);  // ホイールイベントの追加
			pictureBox2.Visible = false;
			InitCombo();
            InitCombo2();
			UpdatePageCtrl(0);
		}

		public static string GetIdOfDomain(string domainName, string ServerName)
		{
			string id = "";
			ManagementObject computer_system = new ManagementObject(string.Format("Win32_ComputerSystem.Name='{0}'", Environment.MachineName));
			object objPartOfDomain = computer_system["PartOfDomain"];
			bool isDmn = (bool)objPartOfDomain;
			if (isDmn)  // 指定したドメインに参加している
			{
				object objDomain = computer_system["Domain"];
				string dName = objDomain.ToString();
				if (dName.ToLower().IndexOf(domainName.ToLower()) != -1)  // ドメイン名を含んでいる
					id = System.Environment.UserName;  // ユーザーネームをIDとして返す
			}
			computer_system.Dispose();
			if (string.IsNullOrEmpty(id))
			{
				// ワークグループまたは別のドメインに参加しているので、資格情報のサーバー情報から確認する
				Credential cm = new Credential();
				cm.Target = ServerName;
				cm.Type = CredentialType.DomainPassword;
				if (cm.Exists())  // 指定したサーバー名の資格情報がある（パスワードを保存している）
				{
					cm.Load();
					id = cm.Username;
					int pos = id.LastIndexOf("\\");  // \の右側を取得
					if (pos != -1)
						id = id.Substring(pos + 1);
				}
				cm.Dispose();
			}
			return id;
		}

		// 捺印用コンボボックス初期化
		private void InitCombo()
		{
			//string id = GetIdOfDomain("domainname", "computername");
			string strCurDir = Path.GetDirectoryName(Application.ExecutablePath);
			strCurDir = Path.Combine(strCurDir, "natsuin");
			if (!Directory.Exists(strCurDir))
			{
				toolStripButton1.Enabled = false;
				toolStripComboBox1.Enabled = false;
				return;
			}
			string[] files = Directory.GetFiles(strCurDir, "*.pdf").Concat(Directory.GetFiles(strCurDir, "*.png")).ToArray();
			for (int i = 0; i < files.Length; i++)
			{
				CmbObject obj = new CmbObject(files[i], Path.GetFileName(files[i]));
				toolStripComboBox1.Items.Add(obj);
			}
			if (toolStripComboBox1.Items.Count > 0)
			{
				if (m_opt.m_NatsuinPos < toolStripComboBox1.Items.Count)
					toolStripComboBox1.SelectedIndex = m_opt.m_NatsuinPos;
				else
					toolStripComboBox1.SelectedIndex = 0;
			}
			else
			{
				toolStripButton1.Enabled = false;
				toolStripComboBox1.Enabled = false;
			}
		}

        // 印影サイズ用コンボボックス初期化
        private void InitCombo2()
        {
            int pos = -1;
            int[] arySize = { 60, 90, 105, 120, 135 };
            for (int i = 0; i < arySize.Length; i++)
            {
                if (arySize[i] == m_opt.m_NatsuinSize)
                    pos = i;
                double data = arySize[i] / 10.0;
                toolStripComboBox2.Items.Add(data.ToString("0.0"));
            }
            toolStripComboBox2.SelectedIndex = pos;
        }

        private void UpdatePageCtrl(int page)
		{
			int maxPage = 0;
			if (page != 0)
				maxPage = m_pdfDoc.PageCount;
			m_pageArray.Init(maxPage, pictureBox1.Left, pictureBox1.Top);
			toolStripLabel1.Text = "/ " + maxPage.ToString();
			SetPage(page);
		}

		private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
		{
			int wcnt = e.Delta / 120;
			if (m_blCtrl)  // Ctrlを押しているので画面拡大縮小
			{
				int page = 0;
				if (GetPage(out page))
				{
					double ratio = m_pageArray.GetPageRatio(page);
					if (ratio != 0.0)
					{
						if (wcnt > 0)
						{
							if (12.0 < ratio)
								return;
							ratio *= 1.1;
						}
						else
						{
							if (ratio < 0.05)
								return;
							ratio /= 1.1;
						}
						m_pageArray.SetPageRatio(page, ratio);
						DispRatio(ratio);
						DisplayPage(page);
						DispPos(pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);
					}
				}
			}
			else  // ページ変更
			{
				int page = 0;
				if (GetPage(out page))
				{
					int page2 = page - wcnt;
					if (m_pageArray.CheckPage(page2) && SetPage(page2))
						DisplayPage(page2);
				}
			}
		}

		private void DispRatio(double ratio)
		{
			toolStripStatusLabel2.Text = string.Format("{0}%", (int)Math.Round(ratio * 100));
		}
		private void DispPos(int x, int y, int w, int h)
		{
			toolStripStatusLabel3.Text = string.Format("x={0}, y={1}, w={2}, h={3}", x, y, w, h);
		}

		private void DispSize(SizeF size, double ratio)
		{
			m_papermm_width = uty.point2mm(size.Width);
			m_papermm_height = uty.point2mm(size.Height);
			double width = Math.Round(m_papermm_width, 1, MidpointRounding.AwayFromZero);
			double height = Math.Round(m_papermm_height, 1, MidpointRounding.AwayFromZero);
			toolStripLabel2.Text = string.Format("サイズ {0}×{1} mm", height, width);
			DispRatio(ratio);
		}

		private void DisplayPage(int page)
		{
			if (page > m_pageArray.m_maxPage)
				return;
			double h1 = m_papermm_width / m_papermm_height;  // pdfの縦横比
			double h2 = (double)pictureBox1.Width / (double)pictureBox1.Height;  // コントロールの縦横比
			if (h2 > 10)  // 落ちないように
				return;

			int width = 0, height = 0;
			int page_width = 0;
			int page_height = 0;
			double page_ratio = 0.0;
			int ret = m_pageArray.GetPageSize(page, ref page_width, ref page_height, ref page_ratio);
			if (ret == -1)
				return;
			else if (ret == 0)  // 初期化されていない（フォームの比率で縦横を決める）
			{
				double pic_width = (double)pictureBox1.Width;
				double pic_height = (double)pictureBox1.Height;
				if (h1 < h2)  // フォーム内にImageを当てはめる判定
				{
					width = (int)Math.Round(pic_height * h1);
					height = (int)Math.Round(pic_height);
				}
				else
				{
					width = (int)Math.Round(pic_width);
					height = (int)Math.Round(pic_width / h1);
				}
				if (width == 0 && height == 0)
					return;
				m_pageArray.SetPageSize(page, width, height, 1.0);
			}
			else  // 初期化されている（設定されている縦横に倍率をかける）
			{
				width = (int)Math.Round((double)page_width * page_ratio);
				height = (int)Math.Round((double)page_height * page_ratio);
				if (width == 0 && height == 0)
					return;
			}
			Bitmap canvas = new Bitmap(width, height);
			Graphics g = Graphics.FromImage(canvas);
			System.Drawing.Image img = m_pdfDoc.Render(page - 1, width, height, 200, 200, PdfRenderFlags.ForPrinting | PdfRenderFlags.Annotations);

			if (height > pictureBox1.Height || width > pictureBox1.Width)
			{
				int p1 = 0, p2 = 0, p3 = img.Width, p4 = img.Height;
				int h = (int)Math.Round((height - pictureBox1.Height) * 0.5);
				if (h > 0)
				{
					p2 += h;
					p4 -= h;
				}
				int w = (int)Math.Round((width - pictureBox1.Width) * 0.5);
				if (w > 0)
				{
					p1 += w;
					p3 -= w;
				}
				g.DrawImage(img, p1, p2, p3, p4);
			}
			else
				g.DrawImage(img, 0, 0, img.Width, img.Height);

			g.Dispose();
			img.Dispose();
			System.Drawing.Image oldImage = pictureBox1.Image;
			pictureBox1.Image = canvas;
			if (oldImage != null)
			{
				oldImage.Dispose();
				oldImage = null;
			}
//			canvas.Dispose();  「使用されたパラメーターが有効ではありません」と出るためコメントアウト
			canvas = null;

//			System.Drawing.Image img = m_pdfDoc.Render(page - 1, width, height, 200, 200, PdfRenderFlags.ForPrinting | PdfRenderFlags.Annotations);
//			System.Drawing.Image oldImage = pictureBox1.Image;
//			pictureBox1.Image = img;
//			if( oldImage != null)
//				oldImage.Dispose();
		}

		private void pictureBox1_ClientSizeChanged(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
			{
				DisplayPage(page);
				DispPos(pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);
			}
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			pictureBox1.Focus();
		}

		private void SaveEdited()
		{
			if (m_EditFlag)  // 編集されている
			{
				System.Windows.Forms.DialogResult res = MessageBox.Show("このPDFは編集されています。保存しますか？", "確認", MessageBoxButtons.YesNo);
				if (res == System.Windows.Forms.DialogResult.Yes)
					fcFile.pdfSave(m_OpenFilePath, ref m_pdfDoc, m_OpenFilePath, m_pdfPsw);
			}
		}

		private void Form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveEdited();
			fcFile.pdfClose(ref m_pdfDoc);
			m_OpenFilePath = "";

			if (!string.IsNullOrWhiteSpace(m_DeleteFile))  // テンポラリファイルは削除
				uty.FileDelete(m_DeleteFile);
			if (pictureBox1.Image != null)
				pictureBox1.Image.Dispose();  // 一応メモリー解放
			m_opt.m_NatsuinPos = toolStripComboBox1.SelectedIndex;

            float fsize = 10.5f;
			string Text = toolStripComboBox2.Items[toolStripComboBox2.SelectedIndex].ToString();
			float.TryParse(Text, out fsize);
            m_opt.m_NatsuinSize = (int)(fsize * 10.0f);
			m_opt.SaveReg();


			pdfArecore.Properties.Settings.Default.viewState = WindowState;
			if (WindowState == FormWindowState.Normal)
			{
				// ウインドウステートがNormalな場合には位置（location）とサイズ（size）を記憶する
				pdfArecore.Properties.Settings.Default.viewLocation = Location;
				pdfArecore.Properties.Settings.Default.viewSize = Size;
			}
			else if (WindowState == FormWindowState.Maximized)
			{
				// 最大化（maximized）の場合には、RestoreBoundsを記憶する
				pdfArecore.Properties.Settings.Default.viewLocation = RestoreBounds.Location;
				pdfArecore.Properties.Settings.Default.viewSize = RestoreBounds.Size;
			}
			else  // 最小化は保存しない
				return;
			pdfArecore.Properties.Settings.Default.Save();
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			if (pdfArecore.Properties.Settings.Default.viewSize.Width == 0)
				pdfArecore.Properties.Settings.Default.Upgrade();
			// もしC#デスクトップアプリをバージョンアップすると、記憶している情報が消え去るが、この↑を
			// 入れておくと引き継がれる（らしい）。

			if (pdfArecore.Properties.Settings.Default.viewSize.Width == 0 || pdfArecore.Properties.Settings.Default.viewSize.Height == 0)
			{
				// 初回起動時にはここに来るので必要なら初期値を与えても良い。
				// 何も与えない場合には、デザイナーウインドウで指定されている大きさになる。
			}
			else
			{
				WindowState = pdfArecore.Properties.Settings.Default.viewState;

				// もし前回終了時に最小化されていても、今回起動時にはNormal状態にしておく
				if (WindowState == FormWindowState.Minimized)
					WindowState = FormWindowState.Normal;
				Location = pdfArecore.Properties.Settings.Default.viewLocation;
				Size = pdfArecore.Properties.Settings.Default.viewSize;
			}
			// コマンドライン読込み
			string[] cmds = System.Environment.GetCommandLineArgs();
			if (cmds.Length < 2)
				return;
			CmdParam param = new CmdParam();
			GetParam(cmds, param);
			if (ExecCmd(param))
				Application.Exit();
		}

		private bool SetPage(int page)
		{
			if (0 <= page && page <= m_pageArray.m_maxPage)
			{
				m_pageArray.m_actPage = page;
				toolStripTextBox1.Text = page.ToString();  // 現在のページ
				toolStripButton2.Enabled = (page == 0 || page == 1) ? false : true;  // ページ減
				toolStripButton3.Enabled = (page == 0 || page == m_pageArray.m_maxPage) ? false : true;  // ページ増
				toolStripDropDownButton2.Enabled = (page == 0) ? false : true;  // ページ編集
				toolStripDropDownButton1.Enabled = (page == 0) ? false : true;  // ページ回転
				toolStripButton1.Enabled = (page == 0) ? false : true;  // 捺印実行
				if (page != 0)
				{
					SizeF size = m_pdfDoc.PageSizes[page - 1];
					double ratio = m_pageArray.GetPageRatio(page);
					DispSize(size, ratio);
				}
				return (page == 0) ? false : true;
			}
			return false;
		}
		private bool GetPage(out int page)
		{
			page = m_pageArray.m_actPage;
			if (1 <= page && page <= m_pageArray.m_maxPage)
				return true;
			return false;
		}

		// 一度保存してファイルにする
		private void SaveTemp()
		{
			if (string.IsNullOrWhiteSpace(m_tempFile))
			{
				m_tempFile = uty.GetTempPDFName();
				m_pdfDoc.Save(m_tempFile);
			}
			m_pdfDoc.Dispose();
		}
		// 処理成功時のリロード
		private void ReloadOfSuccess(int p, bool blEditPage)
		{
			bool stat = false;
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
			finally
			{
				if (m_pdfDoc != null && !m_pdfDoc.Equals(null))
					stat = true;
			}
			if (stat)
			{
				int actPage = p;
				if (blEditPage)
				{
					if (actPage == m_pageArray.m_maxPage)
						actPage = actPage - 1;
					m_pageArray.m_maxPage = m_pdfDoc.PageCount;
					UpdatePageCtrl(actPage);
				}
				DisplayPage(actPage);
				if (!string.IsNullOrWhiteSpace(m_tempFile))
					m_DeleteFile = m_tempFile;
			}
		}
		// カーソル変更
		private void ChangeCursor()
		{
			double imgSizemm = m_opt.m_NatsuinSize / 10.0;
			string ext = Path.GetExtension(m_strNtinFile);
			if (string.Compare(ext, ".pdf", true) == 0 || string.Compare(ext, ".png", true) == 0)
			{
				timer1.Interval = 10;
				timer1.Enabled = true;
				pictureBox2.Visible = true;
				pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
				if (string.Compare(ext, ".pdf", true) == 0)
				{
					Bitmap img = new Bitmap(100, 100);
					Graphics g = Graphics.FromImage(img);
					Pen p = new Pen(Color.Red, 5);
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.DrawEllipse(p, 5, 5, 90, 90);
					p.Dispose();
					g.Dispose();
					pictureBox2.Image = img;
				}
				else if (string.Compare(ext, ".png", true) == 0)
					pictureBox2.Image = Image.FromFile(m_strNtinFile);
				double width = pictureBox1.Image.Width / m_papermm_width * imgSizemm;
				double height = pictureBox1.Image.Height / m_papermm_height * imgSizemm;
				m_CursorX = pictureBox2.Width = (int)width;
				m_CursorY = pictureBox2.Height = (int)height;
			}
		}
		// カーソル戻す
		private void ReverseCursor()
		{
			string ext = Path.GetExtension(m_strNtinFile);
			if (string.Compare(ext, ".pdf", true) == 0 || string.Compare(ext, ".png", true) == 0)
			{
				timer1.Enabled = false;
				pictureBox2.Visible = false;
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			// PictureBoxをカーソルの位置に動かす。カーソルの真ん中に画像が表示されるよう、
			// カーソルを表示しているPictureBoxのサイズの二分の一をX,Yそれぞれ引く
			Point Coordinate = this.PointToClient(Cursor.Position);
			pictureBox2.Location = new Point(Coordinate.X - (m_CursorX / 2), Coordinate.Y - (m_CursorY / 2));

			// 印影プレビューは相対的な大きさとする
			double imgSizemm = m_opt.m_NatsuinSize / 10.0;
			double width = pictureBox1.Image.Width / m_papermm_width * imgSizemm;
			double height = pictureBox1.Image.Height / m_papermm_height * imgSizemm;
			pictureBox2.Width = (int)width;
			pictureBox2.Height = (int)height;
		}

		private void ChangeControl()
		{
			if (m_isMouseClickMode)
			{
				toolStripButton1.Enabled = false;
				toolStripDropDownButton1.Enabled = false;
				toolStripDropDownButton2.Enabled = false;
			}
			else
			{
				toolStripButton1.Enabled = true;
				toolStripDropDownButton1.Enabled = true;
				toolStripDropDownButton2.Enabled = true;
			}
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
			{
				Cursor.Current = m_preCursor;
				m_dragStartPos.X = 0;
				m_dragStartPos.Y = 0;
				int page = 0;
				if (GetPage(out page))
					DisplayPage(page);
			}
		}
		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_blCtrl && m_pageArray.m_actPage != 0 && e.Button == MouseButtons.Middle)  // 中ボタンで画面移動
			{
				Point pos = new Point(e.X - m_dragStartPos.X, e.Y - m_dragStartPos.Y);
				int page = 0;
				if (GetPage(out page))
				{
					int x = 0, y = 0;
					m_pageArray.GetPictPos(page, ref x, ref y);
					if (pos.X != 0 || pos.Y != 0)
					{
						pictureBox1.Left = x + pos.X;
						pictureBox1.Top = y + pos.Y;
						m_pageArray.SetPictPos(page, pictureBox1.Left, pictureBox1.Top);
						DispPos(pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);
//						DisplayPage(page);
//						pictureBox1.Refresh();
//						pictureBox1.Update();
					}
				}
			}
		}
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)  // 中ボタンで画面移動開始
			{
				if (m_blCtrl && m_pageArray.m_actPage != 0)
				{
					m_preCursor = Cursor.Current;
					Cursor.Current = Cursors.NoMove2D;
					m_dragStartPos.X = e.X;
					m_dragStartPos.Y = e.Y;
				}
			}
			else if (e.Button == MouseButtons.Right)  // 右クリックで。。。
			{
			}
			else if (e.Button == MouseButtons.Left)  // 左クリックで捺印実行
			{
				if (!m_isMouseClickMode)
					return;
				int p = 0;
				if (!GetPage(out p))
					return;
				double h1 = m_papermm_width / m_papermm_height;  // pdfの縦横比
				double h2 = (double)pictureBox1.Width / (double)pictureBox1.Height;  // コントロールの縦横比
				int width = 0, height = 0;
				double yohaku_w = 0.0, yohaku_h = 0.0;
				Point pnt = e.Location;
				if (h1 < h2)  // フォーム内にImageを当てはめる判定
				{
					width = (int)Math.Round(pictureBox1.Height * h1);
					yohaku_w = (int)Math.Round((pictureBox1.Width - width)/2.0);  // センタリングされているので左側余白を取得
					height = pictureBox1.Height;
					if (pnt.X < yohaku_w || (yohaku_w + width) < pnt.X || pnt.Y < 0 || height < pnt.Y)
					{
						MessageBox.Show("指定した点が用紙範囲外");
						return;
					}
				}
				else
				{
					width = pictureBox1.Width;
					height = (int)Math.Round(pictureBox1.Width / h1);
					yohaku_h = (int)Math.Round((pictureBox1.Height - height)/2.0);  // センタリングされているので上側余白を取得
					if (pnt.Y < yohaku_h || (yohaku_h + height) < pnt.Y || pnt.X < 0 || width < pnt.X)
					{
						MessageBox.Show("指定した点が用紙範囲外");
						return;
					}
				}
				m_rx = (pnt.X - yohaku_w) / (double)width;
				m_ry = 1.0 - ((pnt.Y - yohaku_h) / (double)height);

				SaveTemp();
				if (its.AddNatsuin(ref m_isFirstNtin, ref m_tempFile, p, m_strNtinFile, m_rx, m_ry, m_opt.m_NatsuinSize / 10.0))
					ReloadOfSuccess(p, false);

				m_EditFlag = true;
				m_isMouseClickMode = false;
				ChangeControl();
				ReverseCursor();
			}
		}

		private void Form2_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyData & Keys.KeyCode) == Keys.ControlKey)
				m_blCtrl = true;

			if (!m_isMouseClickMode)
				return;
			if ((e.KeyData & Keys.KeyCode) == Keys.Escape)
			{
				m_isMouseClickMode = false;
				ChangeControl();
				ReverseCursor();
			}
		}

		private void Form2_KeyUp(object sender, KeyEventArgs e)
		{
			if ((e.KeyData & Keys.KeyCode) == Keys.ControlKey)
				m_blCtrl = false;
		}

		private void toolStripDropDownButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			string strTxt = e.ClickedItem.Text;
			int degree = 0;
			if (strTxt == "左90度")
				degree = 270;
			else if (strTxt == "右90度")
				degree = 90;
			else if (strTxt == "180度")
				degree = 180;

			int p = 0;
			if (!GetPage(out p))
				return;
			SaveTemp();
			if (its.PageRotate(ref m_tempFile, p, degree, m_pdfPsw))
				ReloadOfSuccess(p, false);
			m_EditFlag = true;
		}

		private void toolStripDropDownButton2_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			int p = 0;
			if (!GetPage(out p))
				return;
			string strTxt = e.ClickedItem.Text;
			if (strTxt == "削除")
			{
				SaveTemp();
				int[] pages = new int[1] { p };
				if (its.PageDelete(ref m_tempFile, pages))
					ReloadOfSuccess(p, true);
				m_EditFlag = true;
			}
			else if (strTxt == "挿入")
			{
				using (Form6 f = new Form6(p, m_pageArray.m_maxPage))
				{
					if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						string strFile = null;
						if (f.m_isFile)
						{
							strFile = fcFile.DlgInsertFile(ref m_opt.m_OpenPath);
							if (strFile == null)
								return;
						}
						SaveTemp();
						int page = its.PageInsert(ref m_tempFile, f.m_pageSel, strFile, p);
						if (page != 0)
							ReloadOfSuccess(page, true);
						m_EditFlag = true;
					}
				}
			}
			else if (strTxt == "抽出")
			{
				using (Form5 f = new Form5(p, m_pageArray.m_maxPage))
				{
					if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						string path = fcFile.DlgOutputFile(ref m_opt.m_OpenPath);
						if (path == null)
							return;

						// 抽出対象のページ配列
						int num = f.m_pageTo - f.m_pageFrom + 1;
						int[] pages = new int[num];
						for (int i = 0; i < num; i++)
							pages[i] = f.m_pageFrom + i;

						SaveTemp();
						if (its.PageExtract(ref m_tempFile, f.m_blPageDel, path, pages))
							ReloadOfSuccess(p, true);
						m_EditFlag = true;
					}
				}
			}
		}

		private void toolStripDropDownButton2_DropDownOpening(object sender, EventArgs e)
		{
			if (m_pageArray.m_maxPage <= 1)
			{
				削除ToolStripMenuItem.Enabled = false;
				抽出ToolStripMenuItem.Enabled = false;
			}
		}

		// 捺印
		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			if (toolStripComboBox1.SelectedIndex == -1)
				return;

			// 画面拡大率と位置が変わっていたら初期化
			int page = 0;
			if (GetPage(out page))
			{
				bool isInit = false;
				if (m_pageArray.IsCngRatioRetFst(page))
				{
					isInit = true;
					DispRatio(1.0);
				}
				int x = 0, y = 0;
				if (m_pageArray.IsCngPBPosRetFst(page, ref x, ref y))
				{
					pictureBox1.Left = x;
					pictureBox1.Top = y;
					isInit = true;
				}
				if (isInit)
					DisplayPage(page);
			}

			CmbObject obj = (CmbObject)toolStripComboBox1.SelectedItem;
			m_strNtinFile = obj.fullValue;

			// クリックした位置取得
			m_isMouseClickMode = true;
			ChangeControl();
			ChangeCursor();
		}

		// ページダウン
		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
			{
				int page2 = page - 1;
				if (m_pageArray.CheckPage(page2) && SetPage(page2))
					DisplayPage(page2);
			}
		}
		// ページアップ
		private void toolStripButton3_Click(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
			{
				int page2 = page + 1;
				if (m_pageArray.CheckPage(page2) && SetPage(page2))
					DisplayPage(page2);
			}
		}
		// ページ数入力後にフォーカスアウト
		private void toolStripTextBox1_Leave(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
			{
				int page2 = int.Parse(toolStripTextBox1.Text);
				if (m_pageArray.CheckPage(page2))
				{
					if (SetPage(page2))
						DisplayPage(page2);
				}
				else
				{
					MessageBox.Show("ページ入力値が異常", "エラー");
					toolStripTextBox1.Text = page.ToString();
				}
			}
		}

		private void SetStatusFileName(string str)
		{
			if (str == null)
				toolStripStatusLabel1.Text = "無題";
			else
				toolStripStatusLabel1.Text = str;
		}

		private void CloseFile()
		{
			SaveEdited();
			fcFile.pdfClose(ref m_pdfDoc);
			UpdatePageCtrl(0);
			pictureBox1.Image = null;
			SetStatusFileName(null);

			if (!string.IsNullOrWhiteSpace(m_DeleteFile))  // テンポラリファイルは削除
				uty.FileDelete(m_DeleteFile);
			m_DeleteFile = "";
			m_OpenFile = "";
			m_OpenFilePath = "";
			m_pdfName = "";
			m_tempFile = "";
			m_pageArray.m_maxPage = 0;
			m_pageArray.m_actPage = 0;
			m_pdfPsw = "";
			m_EditFlag = false;
			m_blCtrl = false;

			m_isFirstNtin = false;
			m_isMouseClickMode = false;
			m_rx = 0;
			m_ry = 0;
			m_strNtinFile = "";
			m_CursorX = 0;
			m_CursorY = 0;
			m_papermm_width = 0;
			m_papermm_height = 0;

			SizeF zero = new SizeF(0, 0);
			DispSize(zero, 1.0);
		}

		private void toolStripDropDownButton3_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			string strTxt = e.ClickedItem.Text;
			if (strTxt == "開く")
			{
				CloseFile();
				string file = fcFile.DlgOpenFile(ref m_opt.m_OpenPath);
				if (file == null)
					return;
				if (string.Compare(m_OpenFilePath, file, true) != 0)  // 同じファイルなら何もしない
				{
					m_OpenFile = Path.GetFileNameWithoutExtension(file);
					if (fcFile.pdfOpen(file, ref m_pdfDoc, ref m_pdfPsw))
					{
						UpdatePageCtrl(1);
						DisplayPage(1);
						SetStatusFileName(file);
						m_OpenFilePath = file;
					}
				}
			}
			else if (strTxt == "上書き保存")
			{
				fcFile.pdfSave(m_OpenFilePath, ref m_pdfDoc, m_OpenFilePath, m_pdfPsw);
				m_EditFlag = false;
			}
			else if (strTxt == "名前を付けて保存")
			{
				string file = fcFile.DlgSaveFile(ref m_opt.m_OpenPath, m_OpenFile);
				if (file == null)
					return;
				m_OpenFile = Path.GetFileNameWithoutExtension(file);
				if (fcFile.pdfSave(file, ref m_pdfDoc, m_OpenFilePath, m_pdfPsw))
				{
					SetStatusFileName(file);
					m_OpenFilePath = file;
					m_EditFlag = false;
				}
			}
			else if (strTxt == "閉じる")
				CloseFile();
			else if (strTxt == "画像変換")
			{
				using (Form7 f = new Form7(m_OpenFilePath, m_pdfDoc, m_opt))
				{
					f.ShowDialog();
				}
			}
			else if (strTxt == "テキスト抽出")
			{
				fcFile.TextOut(m_pdfDoc, m_OpenFilePath);
			}
			else if (strTxt == "プロパティ")
			{
				Form4 f = new Form4(m_pdfDoc, m_pdfPsw);
				if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					if (!PdfDocument.Equals(f.m_pdfDoc, m_pdfDoc))  // 更新されている
					{
						m_pdfDoc.Dispose();
						m_pdfDoc = f.m_pdfDoc;
						m_OpenFilePath = f.m_pdfName;
						if (!string.IsNullOrWhiteSpace(f.m_tempFile))
							m_DeleteFile = f.m_tempFile;
						m_EditFlag = true;
					}
				}
				f.Dispose();
			}
			else if (strTxt == "セキュリティ")
			{
				Form8 f = new Form8(m_pdfDoc);
				if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					if (!PdfDocument.Equals(f.m_pdfDoc, m_pdfDoc))  // 更新されている
					{
						m_pdfDoc.Dispose();
						m_pdfDoc = f.m_pdfDoc;
						m_OpenFilePath = f.m_pdfName;
						m_pdfPsw = f.m_pdfPsw;
						if (!string.IsNullOrWhiteSpace(f.m_tempFile))
							m_DeleteFile = f.m_tempFile;
						m_EditFlag = true;
					}
				}
				f.Dispose();
			}
		}

		private void toolStripDropDownButton3_DropDownOpening(object sender, EventArgs e)
		{
			bool blMenu = true;
			if (string.IsNullOrWhiteSpace(m_OpenFilePath))
				blMenu = false;
			上書き保存ToolStripMenuItem.Enabled = blMenu;
			名前を付けて保存ToolStripMenuItem.Enabled = blMenu;
			閉じるToolStripMenuItem.Enabled = blMenu;
			画像変換ToolStripMenuItem.Enabled = blMenu;
			テキスト抽出ToolStripMenuItem.Enabled = blMenu;
			プロパティToolStripMenuItem.Enabled = blMenu;
			セキュリティToolStripMenuItem.Enabled = blMenu;
		}

		private bool ExecCmd(CmdParam param)
		{
			bool blEnd = true;
			if (param.m_blLoad)
			{
				if( fcFile.pdfOpen(param.m_strLoadFile, ref m_pdfDoc, ref m_pdfPsw))
				{
					m_OpenFilePath = param.m_strLoadFile;
					UpdatePageCtrl(1);
					DisplayPage(1);
					SetStatusFileName(param.m_strLoadFile);
				}
				if (param.m_blLoadUi)
					blEnd = false;
			}
			if (param.m_blText && param.m_blLoad)
				fcFile.TextOut(m_pdfDoc, param.m_strTextFile);
			if (param.m_blImage && param.m_blLoad)
				fcImage.Exec(m_pdfDoc, param.m_strLoadFile, param.m_iImageType, param.m_iImageDPI, param.m_iImageJpegQuarity, param.m_iImageBmp2V, (param.m_iImageTiffMulti==0)?false:true);
			return blEnd;
		}
		// パラメータ解析
		private void GetParam(string[] cmds, CmdParam param)
		{
			for (int i = 1; i < cmds.Length; i++)
			{
				string c = cmds[i];
				if (c.Length < 2)
					continue;
				if (cmds.Length == 2 && File.Exists(c) && Path.GetExtension(c).ToLower() == ".pdf")
					Cmd_LoadFile("=" + c, true, param);
				else
				{
					string top2 = c.Substring(0, 2);  // 先頭2文字で判断
					switch (top2)
					{
						case "/L":
							Cmd_LoadFile(c.Substring(2), false, param);
							break;
						case "/U":
							Cmd_LoadFile(c.Substring(2), true, param);
							break;
						case "/T":
							Cmd_ExportText(c.Substring(2), param);
							break;
						case "/G":
							Cmd_ExportImage(c.Substring(2), param);
							break;
						default:
							break;
					}
				}
			}
		}
		private void Cmd_LoadFile(string file, bool ui, CmdParam param)
		{
			if (param.m_blLoad)
				return;
			if (file.Length < 2)
				return;
			if (file.Substring(0, 1) != "=")
				return;
			file = file.Substring(1);
			if (!File.Exists(file))
				return;
			param.m_blLoad = true;
			param.m_strLoadFile = file;
			param.m_blLoadUi = ui;
		}
		private void Cmd_ExportText(string file, CmdParam param)
		{
			if (param.m_blText)
				return;
			param.m_blText = true;
			if (file.Length < 2)
				return;
			if (file.Substring(0, 1) != "=")
				return;
			file = file.Substring(1);
			if (File.Exists(file) && !uty.FileDelete(file))
				return;
			param.m_strTextFile = file;

		}
		private void Cmd_ExportImage(string file, CmdParam param)
		{
			if (param.m_blImage)
				return;
			param.m_blImage = true;
			if (file.Length < 2)
				return;
			if (file.Substring(0, 1) != "=")
				return;
			file = file.Substring(1);
			string[] ary = file.Split(',');
			int data = 0;
			for (int i = 0; i < ary.Length; i++)
			{
				data = int.Parse(ary[i]);
				if (i == 0)
				{
					if (0 < data)
						param.m_iImageDPI = data;
				}
				else if (i == 1)
				{
					if (0 <= data && data <= 6)
						param.m_iImageType = data;
				}
				else if (i == 2)
				{
					if (0 <= data && data <= 100)
						param.m_iImageJpegQuarity = data;
				}
				else if (i == 3)
				{
					if (0 <= data && data <= 255)
						param.m_iImageBmp2V = data;
				}
				else if (i == 4)
				{
					if (0 <= data && data <= 1)
						param.m_iImageTiffMulti = data;
				}
				else
					break;
			}
		}

		private void Form2_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			if (files.Length > 0)
			{
				if (string.Compare(m_OpenFilePath, files[0], true) != 0)  // 同じファイルなら何もしない
				{
					m_OpenFile = Path.GetFileNameWithoutExtension(files[0]);
					if (fcFile.pdfOpen(files[0], ref m_pdfDoc, ref m_pdfPsw))  // 最初のファイルだけ
					{
						UpdatePageCtrl(1);
						DisplayPage(1);
						SetStatusFileName(files[0]);
						m_OpenFilePath = files[0];
					}
				}
			}
		}
		private void Form2_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.All;
			else
				e.Effect = DragDropEffects.None;
		}

		// 捺印サイズコンボが変わった
		private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			double nsize = 0.0;
			string Text = toolStripComboBox2.Items[toolStripComboBox2.SelectedIndex].ToString();
			double.TryParse(Text, out nsize);
			m_opt.m_NatsuinSize = (int)(nsize * 10.0);
		}

		private void 再描画ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
				DisplayPage(page);
		}

		private void 描画リセットToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int page = 0;
			if (GetPage(out page))
			{
				bool isInit = false;
				if (m_pageArray.IsCngRatioRetFst(page))
				{
					isInit = true;
					DispRatio(1.0);
				}
				int x = 0, y = 0;
				if (m_pageArray.IsCngPBPosRetFst(page, ref x, ref y))
				{
					pictureBox1.Left = x;
					pictureBox1.Top = y;
					isInit = true;
				}
				if (isInit)
				{
					DisplayPage(page);
					DispPos(pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);
				}
			}
		}
	}
	// データクラス
	class CmbObject
	{
		public string fullValue { get; set; }
		public string Value { get; set; }
		public CmbObject(string fullValue, string Value)
		{
			this.fullValue = fullValue;
			this.Value = Value;
		}

		public override string ToString()
		{
			return Value;
		}
	}
	public class CmdParam
	{
		public bool m_blLoad;
		public string m_strLoadFile;
		public bool m_blLoadUi;
		public bool m_blText;
		public string m_strTextFile;
		public bool m_blImage;
		public int m_iImageDPI;
		public int m_iImageType;
		public int m_iImageJpegQuarity;
		public int m_iImageBmp2V;
		public int m_iImageTiffMulti;

		public CmdParam()
		{
			m_blLoad = false;
			m_strLoadFile = string.Empty;
			m_blLoadUi = false;
			m_blText = false;
			m_strTextFile = string.Empty;
			m_blImage = false;
			m_iImageDPI = -1;
			m_iImageType = -1;
			m_iImageJpegQuarity = -1;
			m_iImageBmp2V = -1;
			m_iImageTiffMulti = -1;
		}
	}
}
