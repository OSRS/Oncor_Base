using System;

namespace Osrs.Oncor.Excel
{
    public class XlCell : IXlCell
    {
        private uint _cellStyle;
        private Type _cellType;
        private string _cellValue;
        private string _cellDataType;

        public XlCell(uint cellStyle, Type cellType, object cellValue) : this(cellStyle, cellType, cellValue, DocumentFormat.OpenXml.Spreadsheet.CellValues.Number.ToString())
        { }
        public XlCell(uint cellStyle, Type cellType, object cellValue, string cellDataType)
        {
            _cellStyle = cellStyle;
            _cellType = cellType;
            _cellValue = cellValue.ToString();
            _cellDataType = cellDataType;
        }

        public uint CellStyle => _cellStyle;
        public Type CellType => _cellType;
        public string CellValue => _cellValue;
        public string CellDataType => _cellDataType;
    }
}
