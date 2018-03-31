using System;

namespace TestingApp
{
    public static class Parsing
    {
        public static Nullable<DateTime> ParseDate(string value)
        {
            Nullable<DateTime> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                DateTime temp;
                bool success = DateTime.TryParse(value, out temp);
                if (success)
                {
                    result = temp;
                }
            }
            return result;
        }

        public static Nullable<double> ParseDouble(string value)
        {
            Nullable<double> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                double temp;
                bool success = double.TryParse(value, out temp);
                if (success)
                {
                    result = temp;
                }
            }
            return result;
        }

        public static Nullable<bool> ParseBoolean(string value)
        {
            Nullable<bool> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                bool temp;
                bool success = bool.TryParse(value, out temp);
                if (success)
                {
                    result = temp;
                }
            }
            return result;
        }
    }
}
