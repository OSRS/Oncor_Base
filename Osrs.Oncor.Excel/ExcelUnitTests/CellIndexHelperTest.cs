using NUnit.Framework;
using Osrs.Oncor.Excel;

namespace ExcelUnitTests
{
    [TestFixture]
    public class CellIndexHelperTest
    {
        [Test]
        public void Row1Column1Test()
        {
            string expectedCellIndex = "A1";
            string actualCellIndex = Osrs.Oncor.Excel.CellIndexHelper.FormatCellIndex(1, 1);
            Assert.AreEqual(expectedCellIndex, actualCellIndex);
        }
        [Test]
        public void Row3Column27Test()
        {
            string expectedCellIndex = "AA3";
            string actualCellIndex = Osrs.Oncor.Excel.CellIndexHelper.FormatCellIndex(3, 27);
            Assert.AreEqual(expectedCellIndex, actualCellIndex);
        }
        [Test]
        public void Row13Column4096Test()
        {
            string expectedCellIndex = "FAN13";
            string actualCellIndex = Osrs.Oncor.Excel.CellIndexHelper.FormatCellIndex(13, 4096);
            Assert.AreEqual(expectedCellIndex, actualCellIndex);
        }
    }
}
