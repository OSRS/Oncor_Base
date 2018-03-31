using System.Drawing;
using Osrs.Oncor.Excel;
using NUnit.Framework;

namespace ExcelUnitTests
{
    [TestFixture]
    class XlWorksheetTest
    {
        [Test]
        public void WorksheetNameAndTabColorTest()
        {
            Color testColor = Color.OldLace;
            string expectedName = "Sheet 1";
            int expectedTabColor = testColor.ToArgb();
            Osrs.Oncor.Excel.XlColor tabColor = new Osrs.Oncor.Excel.XlColor(expectedTabColor);
            Osrs.Oncor.Excel.XlWorksheet sheet = new Osrs.Oncor.Excel.XlWorksheet(expectedName, tabColor, 1, null);
            int actualTabColor = sheet.TabColor.ArgbColor;
            Assert.AreEqual(expectedTabColor, actualTabColor);
            string actualName = sheet.Name;
            Assert.AreEqual(expectedName, actualName);
        }
    }
}
