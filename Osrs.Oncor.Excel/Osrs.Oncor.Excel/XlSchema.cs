using System;
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class XlSchema : IXlRowData
    {
        public XlSchema()
        {
            Columns = new XlColumns();
        }

        public XlColumns Columns { get; }

        public void AddColumn(string name, Type type, uint style)
        {
            XlColumn column = new XlColumn(name, type, style);
            Columns.AddColumn(column);
        }

        public IEnumerator<IXlCell> GetEnumerator()
        {
            return Columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Columns.GetEnumerator();
        }
    }
}
