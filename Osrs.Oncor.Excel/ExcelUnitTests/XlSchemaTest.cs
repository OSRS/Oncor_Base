using System;
using Osrs.Oncor.Excel;
using NUnit.Framework;

namespace ExcelUnitTests
{
    [TestFixture]
    public class XlSchemaTest
    {
        [Test]
        public void XlSchemaHasNoColumnsTest()
        {
            Osrs.Oncor.Excel.XlSchema schema = new Osrs.Oncor.Excel.XlSchema();
            Assert.AreEqual(schema.Columns.Count, 0);

        }

        [Test]
        public void XlSchemaAddOneColumnTest()
        {
            Osrs.Oncor.Excel.XlSchema schema = new Osrs.Oncor.Excel.XlSchema();
            string expectedName = "Column 1";
            Type expectedType = typeof(double);
            uint expectedStyle = Osrs.Oncor.Excel.StyleSheetHelper.Red;
            schema.AddColumn(expectedName, expectedType, expectedStyle);
            Assert.AreEqual(schema.Columns.Count, 1);
        }
    }
}
