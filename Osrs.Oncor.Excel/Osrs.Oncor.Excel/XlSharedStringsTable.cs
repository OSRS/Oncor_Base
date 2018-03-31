using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Osrs.Oncor.Excel
{
    public class XlSharedStringsTable
    {
        private readonly SharedStringTablePart _shareStringPart;

        public XlSharedStringsTable(WorkbookPart workbookPart, XlWorkbook.SaveContext context)
        {
            if (workbookPart.SharedStringTablePart != null)
            {
                _shareStringPart = workbookPart.SharedStringTablePart;
            }
            else
            {
                _shareStringPart = workbookPart.AddNewPart<SharedStringTablePart>(
                    context.RelIdGenerator.GetNext(XlWorkbook.RelType.Workbook));
                _shareStringPart.SharedStringTable = new SharedStringTable();
            }
        }

        public int LookupStringIndex(string text)
        {
            int stringIndex = 0;
            bool stringMissing = true;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in _shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    stringMissing = false;
                    break;
                }

                stringIndex++;
            }
            if (stringMissing)
            {
                // The text does not exist in the part. Create the SharedStringItem and return its index.
                _shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
                _shareStringPart.SharedStringTable.Save();
            }
            return stringIndex;
        }

        public string this[int index]
        {
            get
            {
                List<SharedStringItem> list = new List<SharedStringItem>(_shareStringPart.SharedStringTable.Elements<SharedStringItem>());
                return list[index].InnerText;
            }
        }
    }
}
