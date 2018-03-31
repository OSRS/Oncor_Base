using System;
using System.Collections.Generic;

namespace Osrs.Oncor.Excel
{
    public class XlWorksheet
    {
        private XlColumns _columns;
        private XlRows _rows;

        public XlWorksheet(string name, XlColor tabColor, int position, XlSchema schema)
        {
            Name = name;
            TabColor = tabColor;
            Position = position;
            Schema = schema;
        }

        public string Name { get; }
        public int Position { get; }
        public XlColor TabColor { get; set; }
        public XlSchema Schema { get; }
        public XlColumns Columns
        {
            get
            {
                XlColumns result = _columns;
                if (result == null && Schema != null)
                {
                    result = Schema.Columns;
                }
                if (result == null)
                {
                    result = new XlColumns();
                }
                return result;
            }
        }

        public void AddColumn(string name, Type type, uint style)
        {
            if (_columns == null)
            {
                _columns = new XlColumns();
            }
            XlColumn column = new XlColumn(name, type, style);
            _columns.AddColumn(column);
        }
        public XlRows Rows => _rows ?? (_rows = new XlRows());

        public void AddRow(int index, Dictionary<string, string> values)
        {
            XlRowData row = new XlRowData();
            Dictionary<string, string> tmp = new Dictionary<string, string>(); //this stinks but handles the case insensitive issue
            foreach(KeyValuePair<string, string> item in values)
            {
                tmp[item.Key.ToLowerInvariant()] = item.Value;
            }
            foreach (XlColumn column in Schema.Columns)
            {
                XlCell cell;
                if (column.CellValue!=null)
                    cell = new XlCell(0U, column.Type, tmp[column.CellValue.ToLowerInvariant()]); //the column names are not lower case internally so the output gets the right casing on save.
                else
                    cell = new XlCell(0U, column.Type, "");
                row.AddCell(cell);
            }
            Rows.AddRow(row, index);
        }

        private List<string> headers = null;
        public Dictionary<string, string> ValueRow(int index)
        {
            Dictionary<string, string> row = new Dictionary<string, string>();
            if (headers==null)
            {
                headers = new List<string>();
                foreach(IXlCell cur in Rows[0])
                {
                    if (cur.CellValue != null)
                        headers.Add(cur.CellValue.ToLowerInvariant());
                    else
                        headers.Add(string.Empty);
                }
            }
            FillRow(row, headers, Rows[index]);
            return row;
        }

        private void FillRow(Dictionary<string, string> row, List<string> headers, IXlRowData dataRow)
        {
            //List<IXlCell> headers = new List<IXlCell>(headerRow); //this is a waste
            List<IXlCell> values = new List<IXlCell>(dataRow);
            int numValues = Math.Min(headers.Count, values.Count); //in case they don't match
            for (int index = 0; index < numValues; index++)
            {
                row[headers[index]] = values[index].CellValue; //note that if there's multiple columns with same name this gets last value for that name
            }
        }
    }
}
