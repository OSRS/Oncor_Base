using System;
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories
{
    public enum SchemaType { MeasurementSchema, LookupSchema }
    public enum NullableType { IsNullable, IsNotNullable }
    public class Schema : IEnumerable<SchemaEntry>
    {
        private readonly string _dtoName;
        private readonly Dictionary<string, SchemaEntry> _entries = new Dictionary<string, SchemaEntry>();
        public SchemaType SchemaType { get; }

        public Schema(SchemaType schemaType, string dtoName)
        {
            SchemaType = schemaType;
            _dtoName = dtoName;
        }

        public void Add(
            string columnName, 
            Type valueType, 
            SchemaEntryType columnType, 
            int columnLength = 0, 
            NullableType nullable = NullableType.IsNullable, 
            IRange range = null)
        {
            if (columnName == null)
                columnName = string.Empty;
            SchemaEntry column = new SchemaEntry(columnName, valueType, columnType, columnLength, nullable, range);
            _entries.Add(columnName.ToLowerInvariant(), column);
        }

        public SchemaEntry this[string key] => _entries[key];

        public object Parse(Dictionary<string, string> values, string fieldName)
        {
            if (fieldName == null)
                fieldName = "";
            SchemaEntry entry = this[fieldName.ToLowerInvariant()]; //another waste
            string value = null;
            if (values.ContainsKey(entry.LowerColumnName))
                value = values[entry.LowerColumnName];
            object result = ParseValue(value, entry);
            return result;
        }

        private object ParseValue(string value, SchemaEntry entry)
        {
            object result;
            string valueType = entry.ValueType.ToString();
            switch (valueType)
            {
                case "System.Double":
                    result = ParseDouble(value, entry);
                    break;
                case "System.String":
                    result = value;
                    break;
                case "System.UInt32":
                    result = ParseUnsignedInteger(value, entry);
                    break;
                case "System.Integer":
                case "System.Int32":
                    result = ParseInteger(value, entry);
                    break;
                case "System.DateTime":
                    result = ParseDate(value, entry);
                    break;
                case "System.Boolean":
                    result = ParseBoolean(value, entry);
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
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

        public Nullable<DateTime> ParseDate(string value, SchemaEntry column)
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
                else
                {
                    value = ParseDate(value);
                    success = DateTime.TryParse(value, out temp);
                    if (success)
                        result = temp;
                    else
                        ReportParseFailure(column);
                }
            }
            return result;
        }

        public Nullable<bool> ParseBoolean(string value, SchemaEntry column)
        {
            Nullable<bool> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.ToLowerInvariant();
                if (value == "1" || value == "t" || value == "y" || value == "true" || value == "yes")
                    result = true;
                else if (value == "0" || value == "f" || value == "n" || value == "false" || value == "no")
                    result = false;
                else
                    ReportParseFailure(column);
            }
            return result;
        }

        public Nullable<double> ParseDouble(string value, SchemaEntry column)
        {
            Nullable<double> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (value.ToLower() == "nan")
                {
                    result = Double.NaN;
                }
                else
                {
                    double temp;
                    bool success = double.TryParse(value, out temp);
                    if (success)
                    {
                        result = temp;
                    }
                    else
                    {
                        ReportParseFailure(column);
                    }
                }
            }
            return result;
        }

        public Nullable<int> ParseInteger(string value, SchemaEntry column)
        {
            Nullable<int> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                int temp;
                bool success = int.TryParse(value, out temp);
                if (success)
                {
                    result = temp;
                }
                else
                {
                    ReportParseFailure(column);
                }
            }
            return result;
        }

        public Nullable<uint> ParseUnsignedInteger(string value, SchemaEntry column)
        {
            Nullable<uint> result = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                uint temp;
                bool success = uint.TryParse(value, out temp);
                if (success)
                {
                    result = temp;
                }
                else
                {
                    ReportParseFailure(column);
                }
            }
            return result;
        }

        public void ValidateField(object fieldValue, string fieldName)
        {
            SchemaEntry entry = this[fieldName.ToLowerInvariant()];
            string valueType = entry.ValueType.ToString();
            switch (valueType)
            {
                case "System.Boolean":
                    ValidateBooleanField((bool?)fieldValue, entry);
                    break;
                case "System.Double":
                    ValidateDoubleField((double?)fieldValue, entry);
                    break;
                case "System.String":
                    ValidateStringField((string)fieldValue, entry);
                    break;
                case "System.Integer":
                    ValidateIntegerField((int?)fieldValue, entry);
                    break;
                case "System.UInt32":
                    ValidateUnsignedIntegerField((uint?)fieldValue, entry);
                    break;
                case "System.DateTime":
                    ValidateDateTimeField((DateTime?)fieldValue, entry);
                    break;
            }
        }

        private void ValidateBooleanField(bool? fieldValue, SchemaEntry column)
        {
            if (fieldValue.HasValue)
            {
                column.IsInRange(fieldValue.Value, _dtoName, ValidationIssues);
            }
            else if (column.ColumnNullable == NullableType.IsNotNullable)
            {
                ReportMissingField(column);
            }
        }

        private void ValidateDoubleField(double? fieldValue, SchemaEntry column)
        {
            if (fieldValue.HasValue)
            {
                column.IsInRange(fieldValue.Value, _dtoName, ValidationIssues);
            }
            else if (column.ColumnNullable == NullableType.IsNotNullable)
            {
                ReportMissingField(column);
            }
        }

        private void ValidateIntegerField(int? fieldValue, SchemaEntry column)
        {
            if (fieldValue.HasValue)
            {
                column.IsInRange(fieldValue.Value, _dtoName, ValidationIssues);
            }
            else if (column.ColumnNullable == NullableType.IsNotNullable)
            {
                ReportMissingField(column);
            }
        }

        private void ValidateUnsignedIntegerField(uint? fieldValue, SchemaEntry column)
        {
            if (fieldValue.HasValue)
            {
                column.IsInRange(fieldValue.Value, _dtoName, ValidationIssues);
            }
            else if (column.ColumnNullable == NullableType.IsNotNullable)
            {
                ReportMissingField(column);
            }
        }

        private void ValidateDateTimeField(DateTime? fieldValue, SchemaEntry column)
        {
            if (column.ColumnNullable == NullableType.IsNotNullable && !fieldValue.HasValue)
            {
                ReportMissingField(column);
            }
        }

        private void ValidateStringField(string fieldValue, SchemaEntry column)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
            {
                if (fieldValue.Length > column.ColumnLength)
                {
                    string message = string.Format("The {0} {1} is {2} long which is longer than {3} characters.", _dtoName, column.ColumnName, fieldValue.Length, column.ColumnLength);
                    ValidationIssues.Add(ValidationIssue.Code.StringTooLongCode, message);
                }
            }
            else if (column.ColumnNullable == NullableType.IsNotNullable)
            {
                ReportMissingField(column);
            }
        }

        public void ValidateMinimumOptionalFields(string dtoName, IEnumerable<bool> optionalFieldsPresent, int minimumCount)
        {
            int actualCount = 0;
            foreach (bool isPresent in optionalFieldsPresent)
            {
                if (isPresent)
                {
                    actualCount++;
                    if (actualCount >= minimumCount)
                        return; //we're done, just stop
                }
            }
            if (actualCount < minimumCount)
            {
                string message = string.Format("The {0} is missing at least one optional value field.", dtoName);
                ValidationIssues.Add(ValidationIssue.Code.MissingRequiredFieldCode, message);
            }
        }

        private ValidationIssue.Code GetIssueCode(SchemaEntryType columnType)
        {
            switch (columnType)
            {
                case SchemaEntryType.ForeignLookupKey:
                case SchemaEntryType.ForeignMeasurementKey:
                    return ValidationIssue.Code.MissingForeignKeyCode;
                case SchemaEntryType.LocalMeasurementKey:
                case SchemaEntryType.LocalLookupKey:
                    return ValidationIssue.Code.MissingUniqueKeyCode;
                default:
                    return ValidationIssue.Code.MissingRequiredFieldCode;
            }
        }

        private void ReportParseFailure(SchemaEntry column)
        {
            string message = string.Format("The {0} {1} contains an invalid format for a {2}.", _dtoName, column.ColumnName, column.ValueType);
            ValidationIssues.Add(ValidationIssue.Code.ValueInvalidFormat, message);
        }

        private void ReportMissingField(SchemaEntry column)
        {
            string message = string.Format("The {0} is missing its {1}.", _dtoName, column.ColumnName);
            ValidationIssue.Code errorCode = GetIssueCode(column.ColumnType);
            ValidationIssues.Add(errorCode, message);
        }
        public ValidationIssues ValidationIssues { get; set; }
        public IEnumerator<SchemaEntry> GetEnumerator()
        {
            return _entries.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
