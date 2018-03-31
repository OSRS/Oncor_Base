using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Osrs.Oncor.Excel
{
    public class XlColumns : IEnumerable<XlColumn>
    {
        readonly List<XlColumn> _columns = new List<XlColumn>();

        public int Count => _columns.Count;

        public void AddColumn(XlColumn column)
        {
            _columns.Add(column);
        }

        public IEnumerator<XlColumn> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        public XlColumn this[int index] => _columns[index];

        public XlColumn this[string name]
        {
            get
            {
                return _columns.FirstOrDefault(column => column.Name == name);
            }
        }
    }
}