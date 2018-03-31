using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.CustomProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Osrs.Oncor.Excel
{
    public partial class XlWorkbook
    {
        private void LoadSpreadsheetDocument(SpreadsheetDocument dSpreadsheet)
        {
            if (dSpreadsheet.CustomFilePropertiesPart != null)
            {
                foreach (var m in dSpreadsheet.CustomFilePropertiesPart.Properties.Elements<CustomDocumentProperty>())
                {
                    String name = m.Name.Value;
                    if (m.VTLPWSTR != null)
                        Properties.AddCustomProperty(name, m.VTLPWSTR.Text);
                }
            }
            WorkbookPart workbookPart = dSpreadsheet.WorkbookPart;
            XlSharedStringsTable stringTable = new XlSharedStringsTable(workbookPart, null);
            var sheets = dSpreadsheet.WorkbookPart.Workbook.Sheets;
            foreach (Sheet dSheet in sheets.OfType<Sheet>())
            {
                var sheetName = dSheet.Name;
                var wsPart = dSpreadsheet.WorkbookPart.GetPartById(dSheet.Id) as WorksheetPart;
                if (wsPart != null)
                {
                    XlWorksheet ws = Worksheets.AddWorksheet(sheetName);
                    using (var reader = OpenXmlReader.Create(wsPart))
                    {
                        // We ignore lots of parts.
                        Type[] ignoredElements = new Type[]
                        {
                            typeof(SheetFormatProperties),
                            typeof(SheetViews),
                            typeof(MergeCells),
                            typeof(AutoFilter),
                            typeof(SheetProtection),
                            typeof(DataValidations),
                            typeof(ConditionalFormatting),
                            typeof(Hyperlinks),
                            typeof(PrintOptions),
                            typeof(PageMargins),
                            typeof(PageSetup),
                            typeof(HeaderFooter),
                            typeof(RowBreaks),
                            typeof(Columns),
                            typeof(ColumnBreaks),
                            typeof(LegacyDrawing),
                            typeof(CustomSheetViews) // Custom sheet views contain its own auto filter data, and more, which should be ignored for now
                        };

                        while (reader.Read())
                        {
                            while (ignoredElements.Contains(reader.ElementType))
                                reader.ReadNextSibling();

                            if (reader.ElementType == typeof(Row))
                            {
                                AddRow(ws, stringTable, (Row)reader.LoadCurrentElement());
                            }
                            else if (reader.ElementType == typeof(SheetProperties))
                            {
                                AddSheetTabColor(ws, (SheetProperties) reader.LoadCurrentElement());
                            }
                        }
                        reader.Close();
                    }
                }
            }
        }

        private void AddSheetTabColor(XlWorksheet worksheet, SheetProperties properties)
        {
            XlColor color = new XlColor(properties.TabColor.Rgb);
            worksheet.TabColor = color;
        }

        private void TempAddTableData(WorksheetPart wsPart, StringValue sheetName, XlSharedStringsTable stringTable)
        {
            Worksheet worksheet = wsPart.Worksheet;
            SheetProperties properties = worksheet.SheetProperties;
            XlColor color = new XlColor(properties.TabColor.Rgb);
            XlWorksheet ws = Worksheets.AddWorksheet(sheetName, color, null);
            List<SheetData> data = new List<SheetData>(worksheet.Elements<SheetData>());
            foreach (SheetData sheetData in data)
            {
                AddTableData(sheetData, ws, stringTable);
            }
        }

        private void AddTableData(SheetData sheetData, XlWorksheet worksheet, XlSharedStringsTable stringTable)
        {
            uint expectedRow = 1;
            bool rowsInOrder = true;
            IEnumerable<Row> rows = sheetData.Elements<Row>();
            foreach (Row row in rows)
            {
                if (rowsInOrder)
                {
                    if (row.RowIndex == expectedRow)
                    {
                        AddRow(worksheet, stringTable, row);
                        expectedRow++;
                    }
                    //else
                    //{
                    //    rowsInOrder = false;
                    //}
                }
            }
        }

        private static void AddRow(XlWorksheet worksheet, XlSharedStringsTable stringTable, Row row)
        {
            int expectedColumn = 1;
            bool columnsInOrder = true;
            XlRowData rowData = new XlRowData();
            IEnumerable<Cell> cells = row.Elements<Cell>();
            foreach (Cell cell in cells)
            {
                //if (columnsInOrder)
                //{
                    if (cell.CellReference == null)
                    {
                        columnsInOrder = false;
                    }
                    else
                    {
                        int[] indexes = CellIndexHelper.IndexesFromReference(cell.CellReference);
                        int rowIndex = indexes[0];
                        int columnIndex = indexes[1];
                        AddCell(rowData, stringTable, cell, columnIndex);
                        if (columnIndex == expectedColumn && rowIndex == row.RowIndex)
                            expectedColumn++;
                        else
                            columnsInOrder = false;
                    }
                    
                //}
            }
            //if (columnsInOrder)
            //{
                worksheet.Rows.AddRow(rowData);
            //}
        }

        private static readonly XlCell dummy = new XlCell(0U, typeof(string), "", CellValues.String.ToString());
        private static void InsertCell(XlRowData rowData, XlCell cell, int intendedIndex)
        {
            intendedIndex--; //indexing starts at 1, not 0
            if (rowData._cells.Count == intendedIndex) //it's the right place, just do an Add
                rowData._cells.Add(cell); 
            else if (rowData._cells.Count > intendedIndex)
                rowData._cells[intendedIndex] = cell;  //this may overwrite a cell in theory
            else
            {
                //we need to skip ahead by adding a bunch of nulls
                while(rowData._cells.Count< intendedIndex)
                {
                    rowData._cells.Add(dummy);
                }
                rowData._cells.Add(cell); //that'll do it!
            }
        }

        private static void AddCell(XlRowData rowData, XlSharedStringsTable stringTable, Cell cell, int intendedIndex)
        {
            uint style = cell.StyleIndex ?? 0U;
            string cellValue = cell.CellValue != null ? cell.CellValue.InnerText : "";
            string datatype = null;
            if (cell.DataType == null)
                datatype = CellValues.Number.ToString();
            else if (cell.DataType.Value == CellValues.SharedString)
            {
                datatype = CellValues.String.ToString();
                int stringIndex = Int32.Parse(cellValue);
                cellValue = stringTable[stringIndex];
            }
            else
                datatype = cell.DataType.Value.ToString();
            XlCell newCell = new XlCell(style, typeof(string), cellValue, datatype);
            InsertCell(rowData, newCell, intendedIndex);
        }
    }
}
