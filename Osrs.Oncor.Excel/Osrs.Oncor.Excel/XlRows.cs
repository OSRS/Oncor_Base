using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class XlRows : IEnumerable<IXlRowData>
    {
        private readonly List<IXlRowData> _rows = new List<IXlRowData>();
        public int Count => _rows.Count;
        public IXlRowData this[int index] => _rows[index];

        public IEnumerator<IXlRowData> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        public void AddRow(IXlRowData row)
        {
            _rows.Add(row);
        }

        internal void AddRow(IXlRowData row, int index)
        {
            if (index == this._rows.Count)
                _rows.Add(row);
            else if (index < this._rows.Count)
                _rows[index] = row; //overwrite
            else
            {
                while(_rows.Count<index)
                {
                    _rows.Add(new XlRowData());
                }
                _rows.Add(row);
            }
        }
    }
}