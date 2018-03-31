using System.Drawing;
using Osrs.Oncor.Excel;
using NUnit.Framework;

namespace ExcelUnitTests
{
  [TestFixture]
  public class XlColorTest
  {
    [Test]
    public void ColorArgbToHexTest()
    {
      Color baseColor = Color.AliceBlue;
      Osrs.Oncor.Excel.XlColor testColor = new Osrs.Oncor.Excel.XlColor(baseColor.ToArgb());
      string expectedHex = HexValue(baseColor);
      string actualHex = testColor.HtmlColor;
      Assert.AreEqual(expectedHex, actualHex);
    }
    [Test]
    public void ColorHexToArgbTest()
    {
      Color baseColor = Color.AliceBlue;
      string hexValue = HexValue(baseColor);
      Osrs.Oncor.Excel.XlColor testColor = new Osrs.Oncor.Excel.XlColor(hexValue);
      int expectedArgb = baseColor.ToArgb();
      int actualArgb = testColor.ArgbColor;
      Assert.AreEqual(expectedArgb, actualArgb);
    }

    private static string HexValue(Color baseColor)
    {
      return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", baseColor.A, baseColor.R, baseColor.G, baseColor.B);
    }
  }
}
