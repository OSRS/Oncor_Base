using System;
using System.Globalization;

namespace Osrs.Oncor.Excel
{
    public class XlColor
    {
        private static XlColor _blueColor;
        private static XlColor _orangeColor;
        private static XlColor _whiteColor;

        public static XlColor White
        {
            get
            {
                if (_whiteColor == null)
                {
                    _whiteColor = new XlColor("FFFFFFFF");
                }
                return _whiteColor;
            }
        }

        public static XlColor Blue
        {
            get
            {
                if (_blueColor == null)
                {
                    _blueColor = new XlColor("FF4472C4");
                }
                return _blueColor;
            }
        }

        public static XlColor Orange
        {
            get
            {
                if (_orangeColor == null)
                {
                    _orangeColor = new XlColor("FFED7D31");
                }
                return _orangeColor;
            }
        }

        public XlColor(int argb)
        {
            ArgbColor = argb;
        }

        public XlColor(string hexString)
        {
            ArgbColor = Int32.Parse(hexString, NumberStyles.HexNumber);
        }

        public string HtmlColor
        {
            get
            {
                byte[] bytes = BitConverter.GetBytes(ArgbColor);
                return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", bytes[3], bytes[2], bytes[1], bytes[0]);
            }
        }

        public int ArgbColor { get; }
    }
}
