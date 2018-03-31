using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class XlWorksheets : IEnumerable<XlWorksheet>
    {
        private readonly List<XlWorksheet> _worksheets = new List<XlWorksheet>();

        public XlWorksheets()
        {
            NextSheetNumber = 1;
        }

        public int Count => _worksheets.Count;

        public XlWorksheet AddWorksheet(string name)
        {
            return AddWorksheet(name, DefaultColor, null);
        }

        public XlWorksheet AddWorksheet(XlColor tabColor)
        {
            return AddWorksheet(DefaultName, tabColor, null);
        }

        public XlWorksheet AddWorksheet(string name, XlColor tabColor, XlSchema schema)
        {
            XlWorksheet sheet = new XlWorksheet(name, tabColor, NextSheetNumber, schema);
            _worksheets.Add(sheet);
            IncrementSheetNumber();
            return sheet;
        }

        public XlWorksheet this[int index] => _worksheets[index];

        private static XlColor DefaultColor => XlColor.White;

        private string DefaultName => string.Format("Sheet {0}", NextSheetNumber);

        private int NextSheetNumber { get; set; }

        private void IncrementSheetNumber()
        {
            NextSheetNumber = NextSheetNumber + 1;
        }

        public IEnumerator<XlWorksheet> GetEnumerator()
        {
            return _worksheets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _worksheets.GetEnumerator();
        }
    }
}
