using Osrs.Oncor.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string fName = "b:\\data\\tmp\\WQ1_WQ_single_year.xlsx";
            string fName = "b:\\data\\tmp\\wq_data_sample.xlsx";
            //if (args.Length == 0)
            //{
            //    WriteWorkbook(fName);
            //    PauseForEffect();
            //}
            ReadWorkbook(fName);
            PauseForEffect();
        }

        private static void ReadWorkbook(string fName)
        {
            Console.WriteLine("Reading workbook named [{0}]", fName);
            XlWorkbook book = new XlWorkbook();
            book.Load(fName);
            foreach (XlWorksheet worksheet in book.Worksheets)
            {
                PrintWorksheet(worksheet);
            }
            foreach (XlCustomProperty property in book.Properties)
            {
                PrintCustomProperty(property);
            }
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }

        private static void WriteWorkbook(string fName)
        {
            Console.WriteLine("Creating workbook named [{0}]", fName);
            XlWorkbook book = new XlWorkbook();
            XlWorksheets sheets = book.Worksheets;
            XlSchema schema = GetDeploymentSchema();
            XlWorksheet sheet = sheets.AddWorksheet("DET_Deployments", XlColor.White, schema);
            GetDeploymentRows(sheet);
            schema = GetMeasurementSchema();
            sheet = sheets.AddWorksheet("DET_Measurements", XlColor.White, schema);
            GetMeasurementRows(sheet);
            schema = GetSiteSchema();
            sheet = sheets.AddWorksheet("LIST_Sites", XlColor.Orange, schema);
            GetSiteRows(sheet);
            schema = GetSensorSchema();
            sheet = sheets.AddWorksheet("LIST_Sensors", XlColor.Orange, schema);
            GetSensorRows(sheet);
            XlCustomProperties properties = book.Properties;
            properties.AddCustomProperty("oncorId", Guid.NewGuid().ToString());
            properties.AddCustomProperty("oncorUser", "Dr. Frank N. Furter, ESQ");
            book.Save(fName);
            Console.WriteLine("Closing workbook named [{0}]", fName);
        }

        #region Deployment Schema
        private static XlSchema GetDeploymentSchema()
        {
            XlSchema schema = new XlSchema();
            schema.AddColumn("DeployCode", typeof(string), StyleSheetHelper.Blue);
            schema.AddColumn("SiteId", typeof(int), StyleSheetHelper.Orange);
            schema.AddColumn("SensorId", typeof(int), StyleSheetHelper.Orange);
            schema.AddColumn("StartDate", typeof(DateTime), StyleSheetHelper.Normal);
            schema.AddColumn("EndDate", typeof(DateTime), StyleSheetHelper.Normal);
            schema.AddColumn("Comments", typeof(string), StyleSheetHelper.Normal);
            return schema;
        }

        private static XlSchema GetMeasurementSchema()
        {
            XlSchema schema = new XlSchema();
            schema.AddColumn("DeployCode", typeof(string), StyleSheetHelper.Blue);
            schema.AddColumn("MeasureDateTime", typeof(DateTime), StyleSheetHelper.Normal);
            schema.AddColumn("Temperature", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("SurfaceElevation", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("pH", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("DO", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("Conductivity", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("Salinity", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("Velocity", typeof(double), StyleSheetHelper.Normal);
            schema.AddColumn("Turbidity", typeof(int), StyleSheetHelper.Normal);
            schema.AddColumn("Color", typeof(string), StyleSheetHelper.Normal);
            return schema;
        }

        private static XlSchema GetSiteSchema()
        {
            XlSchema schema = new XlSchema();
            schema.AddColumn("Key", typeof(string), StyleSheetHelper.Orange);
            schema.AddColumn("SiteAlias", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Name", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("PrincipalName", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Description", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("InternalId", typeof(int), StyleSheetHelper.Normal);
            return schema;
        }

        private static XlSchema GetSensorSchema()
        {
            XlSchema schema = new XlSchema();
            schema.AddColumn("Key", typeof(string), StyleSheetHelper.Orange);
            schema.AddColumn("InstrumentName", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Model", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("InstrumentType", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("InstrumentClass", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("Manufacturer", typeof(string), StyleSheetHelper.Normal);
            schema.AddColumn("SensorList", typeof(string), StyleSheetHelper.Normal);
            return schema;
        }
        #endregion

        #region Deployment Rows
        private static XlRows GetDeploymentRows(XlWorksheet sheet)
        {
            XlRows rows = sheet.Rows;
            AddDummyRows(rows, sheet.Schema);
            return rows;
        }

        private static XlRows GetMeasurementRows(XlWorksheet sheet)
        {
            XlRows rows = sheet.Rows;
            AddDummyRows(rows, sheet.Schema);
            return rows;
        }

        private static XlRows GetSiteRows(XlWorksheet sheet)
        {
            XlRows rows = sheet.Rows;
            AddDummyRows(rows, sheet.Schema);
            return rows;
        }

        private static XlRows GetSensorRows(XlWorksheet sheet)
        {
            XlRows rows = sheet.Rows;
            AddDummyRows(rows, sheet.Schema);
            return rows;
        }

        private static void AddDummyRows(XlRows rows, XlSchema schema)
        {
            for (int rowIndex = 1; rowIndex < 12; rowIndex++)
            {
                IXlRowData rowData = new DummySchemaRowData(schema, rowIndex);
                rows.AddRow(rowData);
            }
        }
        #endregion

        private static void PrintCustomProperty(XlCustomProperty property)
        {
            Console.WriteLine("Custom property Name: {0}, Value: {1}", property.Name, property.Value);
        }

        private static void PrintWorksheet(XlWorksheet worksheet)
        {
            int rowIndex = 1;
            Color color = Color.FromArgb(worksheet.TabColor.ArgbColor);
            Console.WriteLine("Worksheet Name: {0}, Color: {1}, Position: {2}", worksheet.Name, color, worksheet.Position);
            foreach(XlColumn cur in worksheet.Columns)
            {
                Console.Write(cur.Name + ", ");
            }
            Console.WriteLine();
            foreach(IXlRowData row in worksheet.Rows)
            {
                int columnIndex = 1;
                PrintRow(worksheet.ValueRow(rowIndex-1));
                //foreach (IXlCell cell in row)
                //{
                //    Console.WriteLine("Row: {3}, Col: {4}, Cell Value: {0}, Type: {1}, Style: {2}", cell.CellValue, cell.CellType, cell.CellStyle, rowIndex, columnIndex++);
                //}
                rowIndex++;
            }
        }

        private static void PrintRow(Dictionary<string, string> row)
        {
            foreach(string cur in row.Values)
            {
                Console.Write(cur + ", ");
            }
            Console.WriteLine();
        }

        private static void PauseForEffect()
        {
            Console.Write("Hit enter to continue -->");
            Console.ReadLine();
        }

        private class DummySchemaRowData : IXlRowData
        {
            private readonly List<IXlCell> _list = new List<IXlCell>();
            public DummySchemaRowData(XlSchema schema, int rowIndex)
            {
                int columnIndex = 1;
                foreach (XlColumn column in schema.Columns)
                {
                    XlCell cell = new XlCell(0U, column.Type, GetValue(column.Type, rowIndex, columnIndex++));
                    _list.Add(cell);
                }
            }

            private static string GetValue(Type cellType, int rowIndex, int columnIndex)
            {
                string value;
                if (NumericTypes.IsNumeric(cellType))
                {
                    value = ((9 * rowIndex) + columnIndex).ToString();
                }
                else if (cellType.Name == "String")
                {
                    value = string.Format("This is a string at {0},{1}.", rowIndex, columnIndex);
                }
                else if (cellType.Name == "DateTime")
                {
                    DateTime temp = new DateTime(2000, 1, 1);
                    temp = temp.AddDays(rowIndex);
                    temp = temp.AddMonths(columnIndex);
                    value = temp.ToShortDateString();
                }
                else
                {
                    value = string.Format("Unknown type {0}", cellType.Name);
                }
                return value;
            }

            public IEnumerator<IXlCell> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
    }
}
