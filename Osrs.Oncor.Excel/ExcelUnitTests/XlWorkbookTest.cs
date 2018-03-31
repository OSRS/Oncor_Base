using NUnit.Framework;

namespace ExcelUnitTests
{
    [TestFixture]
    public class XlWorkbookTest
    {
        [Test]
        public void WorkbookCreateTest()
        {
            Osrs.Oncor.Excel.XlWorkbook book = new Osrs.Oncor.Excel.XlWorkbook();
            Assert.IsNotNull(book);
        }

        [Test]
        public void WorkbookHasNoSheetsTest()
        {
            Osrs.Oncor.Excel.XlWorkbook book = new Osrs.Oncor.Excel.XlWorkbook();
            Assert.AreEqual(book.Worksheets.Count, 0);
        }
    }
}
