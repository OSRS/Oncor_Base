using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Osrs.Oncor.Excel
{
    public static class CellIndexHelper
    {
        internal static readonly NumberStyles NumberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowExponent;
        internal static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets the column number of a given column letter.
        /// </summary>
        /// <param name="columnLetter"> The column letter to translate into a column number. </param>
        public static int GetColumnNumberFromLetter(string columnLetter)
        {
            if (string.IsNullOrEmpty(columnLetter)) throw new ArgumentNullException("columnLetter");

            int retVal;
            columnLetter = columnLetter.ToUpper();

            //Extra check because we allow users to pass row col positions in as strings
            if (columnLetter[0] <= '9')
            {
                retVal = Int32.Parse(columnLetter, CellIndexHelper.NumberStyle, CellIndexHelper.ParseCulture);
                return retVal;
            }

            int sum = 0;

            for (int i = 0; i < columnLetter.Length; i++)
            {
                sum *= 26;
                sum += (columnLetter[i] - 'A' + 1);
            }

            return sum;
        }

        private static readonly string[] letters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        /// <summary>
        /// 	Gets the column letter of a given column number.
        /// </summary>
        /// <param name="columnNumber"> The column number to translate into a column letter. </param>
        public static string GetColumnLetterFromNumber(int columnNumber)
        {
            columnNumber--; // Adjust for start on column 1
            if (columnNumber <= 25)
            {
                return letters[columnNumber];
            }
            var firstPart = (columnNumber) / 26;
            var remainder = ((columnNumber) % 26) + 1;
            return GetColumnLetterFromNumber(firstPart) + GetColumnLetterFromNumber(remainder);
        }

        public static string FormatCellIndex(int rowNumber, int columnNumber)
        {
            string alphaValue = GetColumnLetterFromNumber(columnNumber);
            return string.Format("{0}{1}", alphaValue, rowNumber);
        }

        public static int[] IndexesFromReference(string reference)
        {
            int[] indexes = new int[2];
            string pattern = "([A-Z]+)([0-9]+)";
            MatchCollection matches = Regex.Matches(reference, pattern);
            string letters = matches[0].Groups[1].Value;
            string numbers = matches[0].Groups[2].Value;
            indexes[0] = Int32.Parse(numbers);
            indexes[1] = GetColumnNumberFromLetter(letters);
            return indexes;
        }
    }
}
