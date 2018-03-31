using System;
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class XlCustomProperties : IEnumerable<XlCustomProperty>
    {
        readonly List<XlCustomProperty> _properties = new List<XlCustomProperty>();

        public int Count { get { return _properties.Count; } }

        public void AddCustomProperty(string name, string value)
        {
            XlCustomProperty property = new XlCustomProperty(name, value);
            _properties.Add(property);
        }

        public IEnumerator<XlCustomProperty> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _properties.GetEnumerator();
        }
    }
}