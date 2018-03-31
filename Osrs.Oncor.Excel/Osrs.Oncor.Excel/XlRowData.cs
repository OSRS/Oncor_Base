using System;
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    class XlRowData : IXlRowData
    {
        internal readonly List<IXlCell> _cells = new List<IXlCell>();
        public int Count => _cells.Count;
        public IXlCell this[int index] => _cells[index];

        public IEnumerator<IXlCell> GetEnumerator()
        {
            return _cells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cells.GetEnumerator();
        }

        public void AddCell(IXlCell cell)
        {
            _cells.Add(cell);
        }
    }
}
