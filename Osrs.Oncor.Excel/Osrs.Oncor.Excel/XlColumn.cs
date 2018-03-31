using System;

namespace Osrs.Oncor.Excel
{
    public class XlColumn : IXlCell
    {
        public XlColumn(string name, Type type, uint style)
        {
            Name = name;
            Type = type;
            Style = style;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
        public uint Style { get; private set; }
        public Type CellType => typeof(string);
        public uint CellStyle => Style;
        public string CellValue => Name;
    }
}