using System;
using System.Collections.Generic;
using System.Text;

namespace Osrs.Oncor.DetFactories.DTOs
{
    public abstract class BaseDTO : IValidatable
    {
        public BaseDTO()
        {
            ValidationIssues = new ValidationIssues();
        }
        public abstract Schema Schema { get; }

        public string FormatString(string value)
        {
            if (value != null)
                return value;
            return "";
            
        }

        public string FormatDate(Nullable<DateTime> value)
        {
            string result = "";
            if (value.HasValue)
            {
                result = value.Value.ToString("s");
            }
            return result;
        }

        public string FormatBoolean(Nullable<bool> value)
        {
            string result = "";
            if (value.HasValue)
            {
                result = value.Value.ToString();
            }
            return result;
        }

        public string FormatDouble(Nullable<double> value)
        {
            string result = "";
            if (value.HasValue)
            {
                result = value.Value.ToString();
            }
            return result;
        }

        public string FormatInteger(Nullable<int> value)
        {
            string result = "";
            if (value.HasValue)
            {
                result = value.Value.ToString();
            }
            return result;
        }

        public string FormatUnsignedInteger(Nullable<uint> value)
        {
            string result = "";
            if (value.HasValue)
            {
                result = value.Value.ToString();
            }
            return result;
        }

        public abstract Dictionary<string, string> Values();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in Values())
            {
                builder.AppendFormat("{0}: {1}, ", pair.Key, pair.Value);
            }
            return builder.ToString();
        }

        public ValidationIssues ValidationIssues { get; private set; }
        public abstract void Validate();
    }
}
