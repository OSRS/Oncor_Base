using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Border = DocumentFormat.OpenXml.Spreadsheet.Border;
using BottomBorder = DocumentFormat.OpenXml.Spreadsheet.BottomBorder;
using Color = DocumentFormat.OpenXml.Spreadsheet.Color;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;
using Fonts = DocumentFormat.OpenXml.Spreadsheet.Fonts;
using FontSize = DocumentFormat.OpenXml.Spreadsheet.FontSize;
using LeftBorder = DocumentFormat.OpenXml.Spreadsheet.LeftBorder;
using RightBorder = DocumentFormat.OpenXml.Spreadsheet.RightBorder;
using TopBorder = DocumentFormat.OpenXml.Spreadsheet.TopBorder;

namespace Osrs.Oncor.Excel
{
    public static class StyleSheetHelper
    {
        public const uint Normal = 0U;
        public const uint Red = 1U;
        public const uint Blue = 2U;
        public const uint Yellow = 3U;
        public const uint Orange = 1U;

        public static Stylesheet CreateStylesheet()
        {
            Stylesheet stylesheet1 = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");

            var fonts1 = AddFonts();
            var fills1 = AddFills();
            var borders1 = AddBorders();

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = 1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U };

            cellStyleFormats1.AppendChild(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = 4U };
            // Black text on White background
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U };
            // White text on Orange background
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = 0U, FontId = 1U, FillId = 2U, BorderId = 0U, FormatId = 0U, ApplyFill = true };
            // White text on Blue background
            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = 0U, FontId = 1U, FillId = 3U, BorderId = 0U, FormatId = 0U, ApplyFill = true };
            // Black text on Yellow background
            CellFormat cellFormat5 = new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 4U, BorderId = 0U, FormatId = 0U, ApplyFill = true };

            cellFormats1.AppendChild(cellFormat2);
            cellFormats1.AppendChild(cellFormat3);
            cellFormats1.AppendChild(cellFormat4);
            cellFormats1.AppendChild(cellFormat5);

            CellStyles cellStyles1 = new CellStyles() { Count = 1U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = 0U, BuiltinId = 0U };

            cellStyles1.AppendChild(cellStyle1);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = 0U };
            TableStyles tableStyles1 = new TableStyles() { Count = 0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleMedium9" };

            StylesheetExtensionList stylesheetExtensionList = new StylesheetExtensionList();
            StylesheetExtension stylesheetExtension = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            stylesheetExtension.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
            DocumentFormat.OpenXml.Office2010.Excel.SlicerStyles slicerStyles = new DocumentFormat.OpenXml.Office2010.Excel.SlicerStyles() { DefaultSlicerStyle = "SlicerStyleLight1" };
            stylesheetExtension.AppendChild(slicerStyles);
            stylesheetExtensionList.AppendChild(stylesheetExtension);


            stylesheet1.AppendChild(fonts1);
            stylesheet1.AppendChild(fills1);
            stylesheet1.AppendChild(borders1);
            stylesheet1.AppendChild(cellStyleFormats1);
            stylesheet1.AppendChild(cellFormats1);
            stylesheet1.AppendChild(cellStyles1);
            stylesheet1.AppendChild(differentialFormats1);
            stylesheet1.AppendChild(tableStyles1);
            stylesheet1.AppendChild(stylesheetExtensionList);
            return stylesheet1;
        }

        private static Borders AddBorders()
        {
            Borders borders1 = new Borders() {Count = 1U};

            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.AppendChild(leftBorder1);
            border1.AppendChild(rightBorder1);
            border1.AppendChild(topBorder1);
            border1.AppendChild(bottomBorder1);
            border1.AppendChild(diagonalBorder1);

            borders1.AppendChild(border1);
            return borders1;
        }

        private static Fills AddFills()
        {
            Fills fills1 = new Fills() {Count = 5U};

            // FillId = 0
            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() {PatternType = PatternValues.None};
            fill1.AppendChild(patternFill1);

            // FillId = 1
            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() {PatternType = PatternValues.Gray125};
            fill2.AppendChild(patternFill2);

            // FillId = 2,ORANGE
            Fill fill3 = new Fill();
            PatternFill patternFill3 = new PatternFill() {PatternType = PatternValues.Solid};
            ForegroundColor foregroundColor1 = new ForegroundColor() {Rgb = "FFED7D31" };
            BackgroundColor backgroundColor1 = new BackgroundColor() {Indexed = 64U};
            patternFill3.AppendChild(foregroundColor1);
            patternFill3.AppendChild(backgroundColor1);
            fill3.AppendChild(patternFill3);

            // FillId = 3,BLUE
            Fill fill4 = new Fill();
            PatternFill patternFill4 = new PatternFill() {PatternType = PatternValues.Solid};
            ForegroundColor foregroundColor2 = new ForegroundColor() {Rgb = "FF4472C4" };
            BackgroundColor backgroundColor2 = new BackgroundColor() { Indexed = 64U };
            patternFill4.AppendChild(foregroundColor2);
            patternFill4.AppendChild(backgroundColor2);
            fill4.AppendChild(patternFill4);

            // FillId = 4,YELLO
            Fill fill5 = new Fill();
            PatternFill patternFill5 = new PatternFill() {PatternType = PatternValues.Solid};
            ForegroundColor foregroundColor3 = new ForegroundColor() {Rgb = "FFFFFF00"};
            BackgroundColor backgroundColor3 = new BackgroundColor() {Indexed = 64U};
            patternFill5.AppendChild(foregroundColor3);
            patternFill5.AppendChild(backgroundColor3);
            fill5.AppendChild(patternFill5);

            fills1.AppendChild(fill1);
            fills1.AppendChild(fill2);
            fills1.AppendChild(fill3);
            fills1.AppendChild(fill4);
            fills1.AppendChild(fill5);
            //fills1.AppendChild(fill6);
            return fills1;
        }

        private static Fonts AddFonts()
        {
            Fonts fonts1 = new Fonts() {Count = 2U, KnownFonts = true};
            CreateThemeFont(fonts1);
            CreateColorFont(fonts1);

            return fonts1;
        }

        private static void CreateThemeFont(Fonts fonts)
        {
            Color color = new Color() { Theme = 1U };
            CreateBasicFont(fonts, color);
        }

        private static void CreateColorFont(Fonts fonts)
        {
            Color color = new Color() { Rgb = new HexBinaryValue() { Value = "FFFFFF"} };
            CreateBasicFont(fonts, color);
        }

        private static void CreateBasicFont(Fonts fonts, Color color)
        {
            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() {Val = 11D};
            FontName fontName1 = new FontName() {Val = "Calibri"};
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() {Val = 2};
            FontScheme fontScheme1 = new FontScheme() {Val = FontSchemeValues.Minor};

            font1.AppendChild(fontSize1);
            font1.AppendChild(color);
            font1.AppendChild(fontName1);
            font1.AppendChild(fontFamilyNumbering1);
            font1.AppendChild(fontScheme1);
            fonts.AppendChild(font1);

        }
    }
}
