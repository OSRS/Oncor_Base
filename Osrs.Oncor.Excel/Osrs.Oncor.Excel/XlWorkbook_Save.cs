using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.CustomProperties;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;

namespace Osrs.Oncor.Excel
{
    public partial class XlWorkbook
    {
        private static SpreadsheetDocument CreateSpreadsheetDocument(string filePath)
        {
            SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            var properties = document.PackageProperties;
            properties.Creator = "ONCOR";
            return document;
        }

        private static SpreadsheetDocument CreateSpreadsheetDocument(Stream stream)
        {
            SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            var properties = document.PackageProperties;
            properties.Creator = "ONCOR";
            return document;
        }

        private void CreateWorkbookContent(WorkbookPart workbookPart, SaveContext context)
        {
            if (workbookPart.Workbook == null)
                workbookPart.Workbook = new Workbook();

            var workbook = workbookPart.Workbook;
            workbook.Save();
            if (
                !workbook.NamespaceDeclarations.Contains(new KeyValuePair<string, string>("r",
                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships")))
            {
                workbook.AddNamespaceDeclaration("r",
                    "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            }

            if (workbook.WorkbookProperties == null)
                workbook.WorkbookProperties = new WorkbookProperties();

            if (workbook.WorkbookProperties.CodeName == null)
                workbook.WorkbookProperties.CodeName = "ThisWorkbook";
            WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>(context.RelIdGenerator.GetNext(RelType.Workbook));
            // add styles to sheet
            wbsp.Stylesheet = StyleSheetHelper.CreateStylesheet();
            wbsp.Stylesheet.Save();

            WriteWorksheets(workbookPart, context);

            // GenerateSharedStringTablePartContent(sharedStringTablePart, context);
            workbook.Save();
        }

        private void WriteCustomProperties(SpreadsheetDocument document)
        {
            if (Properties.Count != 0)
            {
                CustomFilePropertiesPart propertiesPart = document.CustomFilePropertiesPart ?? AddExtended(document);
                GenerateCustomFilePropertiesPartContent(propertiesPart);
            }
        }

        private CustomFilePropertiesPart AddExtended(SpreadsheetDocument document)
        {
            CustomFilePropertiesPart propertiesPart = document.CustomFilePropertiesPart;
            if (propertiesPart == null)
            {
                propertiesPart = document.AddCustomFilePropertiesPart();
                if (propertiesPart != null && propertiesPart.Properties == null)
                {
                    propertiesPart.Properties = new DocumentFormat.OpenXml.CustomProperties.Properties();
                }
            }
            return propertiesPart;
        }

        private void WriteWorksheets(WorkbookPart workbookPart, SaveContext context)
        {
            XlSharedStringsTable stringsTable = new XlSharedStringsTable(workbookPart, context);
            Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
            foreach (XlWorksheet sheet in Worksheets)
            {
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>(context.RelIdGenerator.GetNext(RelType.Workbook));
                SheetData sheetData = new SheetData();
                Worksheet worksheet = new Worksheet(sheetData);
                worksheetPart.Worksheet = worksheet;
                if (
                    !worksheet.NamespaceDeclarations.Contains(new KeyValuePair<String, String>("r",
                        "http://schemas.openxmlformats.org/officeDocument/2006/relationships")))
                {
                    worksheet.AddNamespaceDeclaration("r",
                        "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                }

                var sheetProperties = new SheetProperties {TabColor = new TabColor()};
                string colorString = sheet.TabColor.HtmlColor;
                sheetProperties.TabColor.Rgb = colorString;
                worksheet.SheetProperties = sheetProperties;
                AddTableData(sheet, sheetData, stringsTable);
                worksheet.Save();
                Sheet s = new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (uint) (sheets.Count() + 1),
                    Name = sheet.Name
                };
                sheets.AppendChild(s);

                workbookPart.Workbook.Save();
            }
        }

        private void AddTableData(XlWorksheet sheet, SheetData sheetData, XlSharedStringsTable stringsTable)
        {
            int rowIndex = 1;
            AddRow(sheet.Schema, sheetData, stringsTable, rowIndex++);
            foreach (IXlRowData rowData in sheet.Rows)
            {
                AddRow(rowData, sheetData, stringsTable, rowIndex++);
            }
        }

        private static void AddRow(IXlRowData rowData, SheetData sheetData, XlSharedStringsTable stringsTable, int rowIndex)
        {
            int columnIndex = 1;
            Row row = new Row() {RowIndex = (uint)rowIndex};
            //sheetData.AppendChild(row);
            foreach (IXlCell cell in rowData)
            {
                string cellReference = CellIndexHelper.FormatCellIndex(rowIndex, columnIndex);
                Cell newCell = new Cell() { CellReference = cellReference, StyleIndex = cell.CellStyle };
                row.AppendChild(newCell);
                SetCellValue(cell.CellValue, cell.CellType, newCell, stringsTable);
                columnIndex++;
            }
            sheetData.AppendChild(row); //moved from above to ensure all cell writes are on the "standalone" row then just 1 DOM manip to insert row into sheet.
        }

        private static void SetCellValue(string cellValue, Type cellType, Cell newCell, XlSharedStringsTable stringsTable)
        {
            if (NumericTypes.IsNumeric(cellType))
            {
                newCell.CellValue = new CellValue(cellValue.ToString());
                newCell.DataType = new EnumValue<CellValues>(CellValues.Number);
            }
            else if ((cellType.Name == "String") || (cellType.Name == "DateTime"))
            {
                // You can only mark a DateTime as a CellValues.Date if
                // It conforms to a specific ISO 8601 format.
                // It then is displayed as a number.  So for the best presentation,
                // we treat them as strings.
                int stringIndex = stringsTable.LookupStringIndex(cellValue);
                newCell.CellValue = new CellValue(stringIndex.ToString());
                newCell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            }
        }

        private void GenerateExtendedFilePropertiesPartContent(ExtendedFilePropertiesPart extendedFilePropertiesPart)
        {
            if (extendedFilePropertiesPart.Properties == null)
                extendedFilePropertiesPart.Properties = new DocumentFormat.OpenXml.ExtendedProperties.Properties();

            var properties = extendedFilePropertiesPart.Properties;
            if (
                !properties.NamespaceDeclarations.Contains(new KeyValuePair<string, string>("vt",
                    "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes")))
            {
                properties.AddNamespaceDeclaration("vt",
                    "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes");
            }

            if (properties.Application == null)
                properties.AppendChild(new Application { Text = "Microsoft Excel" });

            if (properties.DocumentSecurity == null)
                properties.AppendChild(new DocumentSecurity { Text = "0" });

            if (properties.ScaleCrop == null)
                properties.AppendChild(new ScaleCrop { Text = "false" });

            if (properties.HeadingPairs == null)
                properties.HeadingPairs = new HeadingPairs();

            if (properties.TitlesOfParts == null)
                properties.TitlesOfParts = new TitlesOfParts();

            properties.HeadingPairs.VTVector = new VTVector { BaseType = VectorBaseValues.Variant };

            properties.TitlesOfParts.VTVector = new VTVector { BaseType = VectorBaseValues.Lpstr };

            var vTVectorOne = properties.HeadingPairs.VTVector;

            var vTVectorTwo = properties.TitlesOfParts.VTVector;

            var modifiedWorksheets =
                Worksheets.Select(w => new { w.Name, Order = w.Position }).ToList();
            var modifiedWorksheetsCount = modifiedWorksheets.Count;

            InsertOnVtVector(vTVectorOne, "Worksheets", 0, modifiedWorksheetsCount.ToString());

            vTVectorTwo.Size = (UInt32)(modifiedWorksheetsCount);

            foreach (var vTlpstr3 in modifiedWorksheets.OrderBy(w => w.Order).Select(w => new VTLPSTR { Text = w.Name }))
                vTVectorTwo.AppendChild(vTlpstr3);
        }

        private static void InsertOnVtVector(VTVector vTVector, String property, Int32 index, String text)
        {
            var m = from e1 in vTVector.Elements<Variant>()
                where e1.Elements<VTLPSTR>().Any(e2 => e2.Text == property)
                select e1;
            if (!m.Any())
            {
                if (vTVector.Size == null)
                    vTVector.Size = new UInt32Value(0U);

                vTVector.Size += 2U;
                var variant1 = new Variant();
                var vTlpstr1 = new VTLPSTR { Text = property };
                variant1.AppendChild(vTlpstr1);
                vTVector.InsertAt(variant1, index);

                var variant2 = new Variant();
                var vTInt321 = new VTInt32();
                variant2.AppendChild(vTInt321);
                vTVector.InsertAt(variant2, index + 1);
            }

            var targetIndex = 0;
            foreach (var e in vTVector.Elements<Variant>())
            {
                if (e.Elements<VTLPSTR>().Any(e2 => e2.Text == property))
                {
                    vTVector.ElementAt(targetIndex + 1).GetFirstChild<VTInt32>().Text = text;
                    break;
                }
                targetIndex++;
            }
        }

        private void GenerateCustomFilePropertiesPartContent(CustomFilePropertiesPart customFilePropertiesPart1)
        {
            var properties2 = new DocumentFormat.OpenXml.CustomProperties.Properties();
            properties2.AddNamespaceDeclaration("vt",
                "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes");
            var propertyId = 1;
            foreach (XlCustomProperty p in Properties)
            {
                propertyId++;
                var customDocumentProperty = new CustomDocumentProperty
                {
                    FormatId = "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}",
                    PropertyId = propertyId,
                    Name = p.Name
                };
                var vTlpwstr1 = new VTLPWSTR { Text = p.Value };
                customDocumentProperty.AppendChild(vTlpwstr1);
                properties2.AppendChild(customDocumentProperty);
            }

            customFilePropertiesPart1.Properties = properties2;
            customFilePropertiesPart1.Properties.Save();
        }
    }
}
