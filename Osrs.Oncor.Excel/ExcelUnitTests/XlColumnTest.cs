using System;
using Osrs.Oncor.Excel;
using NUnit.Framework;

namespace ExcelUnitTests
{
    [TestFixture]
    class XlColumnTest
    {
        [Test]
        public void XlColumnNameAndTypeTest()
        {
            string expectedName = "Column 1";
            Type expectedType = typeof(int);
            uint expectedStyle = Osrs.Oncor.Excel.StyleSheetHelper.Red;
            Osrs.Oncor.Excel.XlColumn column = new Osrs.Oncor.Excel.XlColumn(expectedName, expectedType, expectedStyle);
            string actualName = column.Name;
            Assert.AreEqual(expectedName, actualName);
            Type actualType = column.Type;
            Assert.AreEqual(expectedType, actualType);
            uint actualStyle = column.Style;
            Assert.AreEqual(expectedStyle, actualStyle);
        }
    }
}
