using System;
using System.Collections.Generic;
using System.IO;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs.DETs
{
    public sealed class ExcelBaseDet
    {
        public delegate void LoadRow(string sheetName, Dictionary<string, string> values);
        public delegate void AddSheetMethod(string sheetName);
        public delegate void CheckSheetCount();
        public delegate void CheckHeaders(XlWorksheet worksheet);
        private readonly List<string> sheetCount;

        internal static List<string> Headers(XlWorksheet worksheet)
        {
            List<string> headers = new List<string>();
            IXlRowData row = worksheet.Rows[0];
            foreach (IXlCell o in row)
            {
                headers.Add(o.CellValue);
            }
            return headers;
        }

        internal static bool HasHeaders(List<string> headers, List<string> expected)
        {
            for(int i=0;i<headers.Count;i++)
            {
                if (headers[i] != null)
                    headers[i] = headers[i].ToLowerInvariant();
                else
                    headers[i] = "";
            }
            foreach(string cur in expected) //we know expected is already lower cased
            {
                if (!headers.Contains(cur))
                    return false;
            }
            return true;
        }

        public static DateTime FromOADate(double date)
        {
            return new DateTime(DoubleDateToTicks(date), DateTimeKind.Unspecified);
        }

        public static long DoubleDateToTicks(double value)
        {
            if (value >= 2958466.0 || value <= -657435.0)
                throw new ArgumentException("Not a valid value");
            long num1 = (long)(value * 86400000.0 + (value >= 0.0 ? 0.5 : -0.5));
            if (num1 < 0L)
                num1 -= num1 % 86400000L * 2L;
            long num2 = num1 + 59926435200000L;
            if (num2 < 0L || num2 >= 315537897600000L)
                throw new ArgumentException("Not a valid value");
            return num2 * 10000L;
        }

        public static string ParseDate(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                DateTime res;
                if (DateTime.TryParse(value, out res))
                    return res.ToString();
                double rl;
                if (double.TryParse(value, out rl))
                    return FromOADate(rl).ToString();
            }
            return string.Empty;
        }

        public ExcelBaseDet(Guid fileId, string owner)
        {
            Id = fileId;
            Owner = owner;
            sheetCount = new List<string>();
        }

        public ExcelBaseDet() { }

        public Guid Id { get; set; }
        public string Owner { get; set; }

        public void Save(Stream stream, IEnumerable<DataTab> tables)
        {
            var book = CreateWorkBook(tables);
            book.Save(stream);
        }

        public void Save(string filename, IEnumerable<DataTab> tables)
        {
            var book = CreateWorkBook(tables);
            book.Save(filename);
        }

        private XlWorkbook CreateWorkBook(IEnumerable<DataTab> tables)
        {
            XlWorkbook book = new XlWorkbook();
            foreach (DataTab tab in tables)
            {
                AddRows(book, tab);
            }
            XlCustomProperties properties = book.Properties;
            properties.AddCustomProperty("oncorId", Id.ToString());
            properties.AddCustomProperty("oncorUser", Owner);
            return book;
        }

        public void OpenWorkbook(Stream stream, LoadRow loadRow, CheckSheetCount checkSheetCount, CheckHeaders checkHeaders)
        {
            XlWorkbook book = new XlWorkbook();
            book.Load(stream);
            Load(book, AddSheet, loadRow);
            checkSheetCount();
            if (checkHeaders != null)
            {
                foreach (XlWorksheet worksheet in book.Worksheets)
                {
                    checkHeaders(worksheet);
                }
            }
        }

        public void OpenWorkbook(string filename, LoadRow loadRow, CheckSheetCount checkSheetCount, CheckHeaders checkHeaders)
        {
            XlWorkbook book = new XlWorkbook();
            book.Load(filename);
            Load(book, AddSheet, loadRow);
            checkSheetCount();
            if (checkHeaders != null)
            {
                foreach (XlWorksheet worksheet in book.Worksheets)
                {
                    checkHeaders(worksheet);
                }
            }
        }

        public void AddSheet(string sheetName)
        {
            if (!sheetCount.Contains(sheetName))
            {
                sheetCount.Add(sheetName);
            }
        }

        public void CheckOneSheet(string sheetName, ValidationIssues issues)
        {
            if (!sheetCount.Contains(sheetName))
            {
                issues.Add(ValidationIssue.Code.MissingDataTab, string.Format("The data tab {0} is missing.", sheetName));
            }
        }

        private void Load(XlWorkbook book, AddSheetMethod addSheet, LoadRow loadRow)
        {
            LoadProperties(book);
            LoadSheets(book, addSheet, loadRow);
        }

        private void LoadProperties(XlWorkbook book)
        {
            foreach (XlCustomProperty property in book.Properties)
            {
                if (property.Name == "oncorId")
                {
                    string guidValue = property.Value;
                    Id = Guid.Parse(guidValue);
                }
                if (property.Name == "oncorUser")
                {
                    Owner = property.Value;
                }
            }
        }

        private void LoadSheets(XlWorkbook book, AddSheetMethod addSheet, LoadRow loadRow)
        {
            foreach (XlWorksheet worksheet in book.Worksheets)
            {
                addSheet(worksheet.Name);
                LoadRows(worksheet, loadRow);
            }
        }

        private void LoadRows(XlWorksheet worksheet, LoadRow loadRow)
        {
            for (int index = 1; index < worksheet.Rows.Count; index++)
            {
                var values = worksheet.ValueRow(index);
                if (values.Count > 0)
                {
                    // Skip empty rows
                    foreach (String cur in values.Values)
                    {
                        if (!string.IsNullOrEmpty(cur)) 
                        {
                            loadRow(worksheet.Name, values); //we got at least one non-empty cell
                            break;
                        }
                    }
                }
            }
        }

        private void AddRows(XlWorkbook book, DataTab data)
        {
            XlWorksheets sheets = book.Worksheets;
            List<BaseDTO> list = new List<BaseDTO>(data);
            XlWorksheet sheet = sheets.AddWorksheet(data.Name, data.Color, data.Schema);
            int numberRows = list.Count;
            for (int rowIndex = 0; rowIndex < numberRows; rowIndex++)
            {
                Dictionary<string, string> values = list[rowIndex].Values();
                sheet.AddRow(rowIndex, values);
            }
        }
    }
}
