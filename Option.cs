using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFiumTest
{
	public class Option
	{
		const string m_RegBase = @"Software\pdfArecore\";
		const string m_RegGrpMain = "main";
		const string m_RegGrpMainOpenPath = "OpenPath";
		const string m_RegGrpMainImageKzd = "ImageKzd";
		const string m_RegGrpMainImageType = "ImageType";
		const string m_RegGrpMainImageJpegQ = "ImageJpegQ";
		const string m_RegGrpMainImageBmp2V = "ImageBmp2V";
		const string m_RegGrpMainImageTiffMulti = "ImageTiffMulti";
		const string m_RegGrpMainNatsuinPos = "NatsuinPos";
        const string m_RegGrpMainNatsuinSize = "NatsuinSize";

        public string m_OpenPath = @"C:\";
		public int m_ImageKzd = 300;
		public int m_ImageType = 3;  // Jpeg
		public int m_ImageJpegQ = 70;
		public int m_ImageBmp2V = 128;
		public int m_ImageTiffMulti = 0;
		public int m_NatsuinPos = 0;
        public int m_NatsuinSize = 105;  // mm×10

        public Option()
		{
		}

		public void LoadReg()
		{
			string subkey = m_RegBase + m_RegGrpMain;
			Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(subkey, false);
			if (regkey == null)
				return;
			string strValue = (string)regkey.GetValue(m_RegGrpMainOpenPath);
			if (strValue != null)
				m_OpenPath = strValue;
			int? iValue = (int?)regkey.GetValue(m_RegGrpMainImageKzd);
			if (iValue != null)
				m_ImageKzd = (int)iValue;
			iValue = (int?)regkey.GetValue(m_RegGrpMainImageType);
			if (iValue != null)
				m_ImageType = (int)iValue;
			iValue = (int?)regkey.GetValue(m_RegGrpMainImageJpegQ);
			if (iValue != null)
				m_ImageJpegQ = (int)iValue;
			iValue = (int?)regkey.GetValue(m_RegGrpMainImageBmp2V);
			if (iValue != null)
				m_ImageBmp2V = (int)iValue;
			iValue = (int?)regkey.GetValue(m_RegGrpMainImageTiffMulti);
			if (iValue != null)
				m_ImageTiffMulti = (int)iValue;
			iValue = (int?)regkey.GetValue(m_RegGrpMainNatsuinPos);
			if (iValue != null)
				m_NatsuinPos = (int)iValue;
            iValue = (int?)regkey.GetValue(m_RegGrpMainNatsuinSize);
            if (iValue != null)
                m_NatsuinSize = (int)iValue;
            regkey.Close();
		}
		public void SaveReg()
		{
			string subkey = m_RegBase + m_RegGrpMain;
			Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(subkey);
			regkey.SetValue(m_RegGrpMainOpenPath, m_OpenPath);
			regkey.SetValue(m_RegGrpMainImageType, m_ImageType);
			regkey.SetValue(m_RegGrpMainImageKzd, m_ImageKzd);
			regkey.SetValue(m_RegGrpMainImageJpegQ, m_ImageJpegQ);
			regkey.SetValue(m_RegGrpMainImageBmp2V, m_ImageBmp2V);
			regkey.SetValue(m_RegGrpMainImageTiffMulti, m_ImageTiffMulti);
			regkey.SetValue(m_RegGrpMainNatsuinPos, m_NatsuinPos);
            regkey.SetValue(m_RegGrpMainNatsuinSize, m_NatsuinSize);
            regkey.Close();
		}
	}
}
