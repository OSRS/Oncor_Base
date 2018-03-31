using System.Drawing;
using Osrs.Oncor.Excel;
using NUnit.Framework;

namespace ExcelUnitTests
{
    [TestFixture]
    public class XlWorksheetsTest
    {
        [Test]
        public void WorksheetsZeroCountTest()
        {
            Osrs.Oncor.Excel.XlWorksheets sheets = new Osrs.Oncor.Excel.XlWorksheets();
            Assert.AreEqual(sheets.Count, 0);
        }
        [Test]
        public void WorksheetsAddOneCountTest()
        {
            Osrs.Oncor.Excel.XlWorksheets sheets = new Osrs.Oncor.Excel.XlWorksheets();
            sheets.AddWorksheet("Sheet 1");
            Assert.AreEqual(sheets.Count, 1);
        }

        [Test]
        public void WorksheetFirstDefaultNameTest()
        {
            Color testColor = Color.OldLace;
            string expectedName = "Sheet 1";
            int expectedTabColor = testColor.ToArgb();
            Osrs.Oncor.Excel.XlColor tabColor = new Osrs.Oncor.Excel.XlColor(expectedTabColor);
            Osrs.Oncor.Excel.XlWorksheets sheets = new Osrs.Oncor.Excel.XlWorksheets();
            Osrs.Oncor.Excel.XlWorksheet sheet = sheets.AddWorksheet(tabColor);
            string actualName = sheet.Name;
            Assert.AreEqual(expectedName, actualName);
        }

        [Test]
        public void WorksheetSecondDefaultNameTest()
        {
            Color testColor = Color.OldLace;
            string expectedName = "Sheet 2";
            int expectedTabColor = testColor.ToArgb();
            Osrs.Oncor.Excel.XlColor tabColor = new Osrs.Oncor.Excel.XlColor(expectedTabColor);
            Osrs.Oncor.Excel.XlWorksheets sheets = new Osrs.Oncor.Excel.XlWorksheets();
            Osrs.Oncor.Excel.XlWorksheet sheet1 = sheets.AddWorksheet(tabColor);
            Osrs.Oncor.Excel.XlWorksheet sheet2 = sheets.AddWorksheet(tabColor);
            string actualName = sheet2.Name;
            Assert.AreEqual(expectedName, actualName);
        }
    }
}
