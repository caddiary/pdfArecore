using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFiumTest
{
	public class PageInfo
	{
		public bool m_blInit = false;
		public int m_iWidth = 0;
		public int m_iHeight = 0;
		public double m_dDispRatio = 1.0;
		public int m_posPicX = 0;
		public int m_posPicY = 0;

		public PageInfo()
		{
		}
		~PageInfo()
		{
		}
	}

	public class PageArray
	{
		public int m_maxPage = 0;
		public int m_actPage = 0;
		private int m_posFstPicX = 0;
		private int m_posFstPicY = 0;
		public PageInfo[] m_Page = new PageInfo[0];

		public PageArray()
		{
		}
		~PageArray()
		{
			m_Page = null;
		}
		public void Init(int num, int pictx, int picty)
		{
			m_maxPage = num;
			if (m_Page.Length > 0)
				Array.Clear(m_Page, 0, m_Page.Length);
			Array.Resize(ref m_Page, num);
			if (num != 0)
			{
				m_posFstPicX = pictx;
				m_posFstPicY = picty;
				for (int i = 0; i < m_Page.Length; i++)
				{
					m_Page[i] = new PageInfo();
					m_Page[i].m_posPicX = pictx;
					m_Page[i].m_posPicY = picty;
				}
			}
		}

		// ページ
		public void SetPageSize(int page, int width, int height, double ratio)
		{
			if (CheckPage(page))
			{
				m_Page[page - 1].m_blInit = true;
				m_Page[page - 1].m_iWidth = width;
				m_Page[page - 1].m_iHeight = height;
				m_Page[page - 1].m_dDispRatio = ratio;
			}
		}
		public int GetPageSize(int page, ref int width, ref int height, ref double ratio)
		{
			if (!CheckPage(page))
				return -1;
			if (!m_Page[page - 1].m_blInit)  // 初期化されていない
				return 0;
			width = m_Page[page - 1].m_iWidth;
			height = m_Page[page - 1].m_iHeight;
			ratio = m_Page[page - 1].m_dDispRatio;
			return 1;
		}
		public bool CheckPage(int page)
		{
			if (1 <= page && page <= m_maxPage)
				return true;
			return false;
		}

		// 画面拡大率
		public double GetPageRatio(int page)
		{
			double ret = 0.0;
			if (CheckPage(page))
				ret = m_Page[page - 1].m_dDispRatio;
			return ret;
		}
		public void SetPageRatio(int page, double ratio)
		{
			if (CheckPage(page))
				m_Page[page - 1].m_dDispRatio = ratio;
		}
		public bool IsCngRatioRetFst(int page)
		{
			bool stat = false;
			if (CheckPage(page))
			{
				if (1.0 != m_Page[page - 1].m_dDispRatio)
				{
					m_Page[page - 1].m_dDispRatio = 1.0;
					stat = true;
				}
			}
			return stat;
		}

		// ピクチャーボックスの位置
		public void GetPictPos(int page, ref int x, ref int y)
		{
			if (CheckPage(page))
			{
				x = m_Page[page - 1].m_posPicX;
				y = m_Page[page - 1].m_posPicY;
			}
		}
		public void SetPictPos(int page, int x, int y)
		{
			if (CheckPage(page))
			{
				m_Page[page - 1].m_posPicX = x;
				m_Page[page - 1].m_posPicY = y;
			}
		}
		public bool IsCngPBPosRetFst(int page, ref int x, ref int y)
		{
			bool stat = false;
			if (CheckPage(page))
			{
				if (m_posFstPicX != m_Page[page - 1].m_posPicX || m_posFstPicY != m_Page[page - 1].m_posPicY)
				{
					x = m_Page[page - 1].m_posPicX = m_posFstPicX;
					y = m_Page[page - 1].m_posPicY = m_posFstPicY;
					stat = true;
				}
			}
			return stat;
		}
	}
}