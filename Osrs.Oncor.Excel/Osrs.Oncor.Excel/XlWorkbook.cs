using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.CustomProperties;
using System;

namespace Osrs.Oncor.Excel
{
    public partial class XlWorkbook
    {
        public XlWorkbook()
        {
            Worksheets = new XlWorksheets();
            Properties = new XlCustomProperties();
        }

        public XlWorksheets Worksheets { get; private set; }

        public XlCustomProperties Properties { get; private set; }

        public void Save(Stream stream)
        {
            var document = CreateSpreadsheetDocument(stream);
            SaveDocument(document);
        }

        public void Save(string filePath)
        {
            var document = CreateSpreadsheetDocument(filePath);
            SaveDocument(document);
        }

        private void SaveDocument(SpreadsheetDocument document)
        {
            SaveContext context = new SaveContext();
            WorkbookPart workbookPart = document.AddWorkbookPart();
            foreach (IdPartPair pair in workbookPart.Parts)
            {
                workbookPart.ChangeIdOfPart(pair.OpenXmlPart, context.RelIdGenerator.GetNext(RelType.Workbook));
            }
            var extendedFilePropertiesPart = document.ExtendedFilePropertiesPart ??
                                             document.AddNewPart<ExtendedFilePropertiesPart>(
                                                 context.RelIdGenerator.GetNext(RelType.Workbook));

            GenerateExtendedFilePropertiesPartContent(extendedFilePropertiesPart);
            CreateWorkbookContent(workbookPart, context);
            WriteCustomProperties(document);
            document.Save();
            document.Close();
        }

        public static XlCustomProperties LoadProperties(Stream stream)
        {
            XlCustomProperties Props = new XlCustomProperties();
            using (var dSpreadsheet = SpreadsheetDocument.Open(stream, false))
            {
                if (dSpreadsheet.CustomFilePropertiesPart != null)
                {
                    foreach (var m in dSpreadsheet.CustomFilePropertiesPart.Properties.Elements<CustomDocumentProperty>())
                    {
                        String name = m.Name.Value;
                        if (m.VTLPWSTR != null)
                            Props.AddCustomProperty(name, m.VTLPWSTR.Text);
                    }
                }
            }
            return Props;
        }

        public static XlCustomProperties LoadProperties(string fileName)
        {
            XlCustomProperties Props = new XlCustomProperties();
            using (var dSpreadsheet = SpreadsheetDocument.Open(fileName, false))
            {
                if (dSpreadsheet.CustomFilePropertiesPart != null)
                {
                    foreach (var m in dSpreadsheet.CustomFilePropertiesPart.Properties.Elements<CustomDocumentProperty>())
                    {
                        String name = m.Name.Value;
                        if (m.VTLPWSTR != null)
                            Props.AddCustomProperty(name, m.VTLPWSTR.Text);
                    }
                }
            }
            return Props;
        }

        public void Load(Stream stream)
        {
            using (var dSpreadsheet = SpreadsheetDocument.Open(stream, false))
                LoadSpreadsheetDocument(dSpreadsheet);
        }

        public void Load(string fileName)
        {
            using (var dSpreadsheet = SpreadsheetDocument.Open(fileName, false))
                LoadSpreadsheetDocument(dSpreadsheet);
        }
    }
}
