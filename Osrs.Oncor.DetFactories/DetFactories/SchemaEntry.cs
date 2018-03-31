using System;

namespace Osrs.Oncor.DetFactories
{
    public class SchemaEntry
    {
        public SchemaEntry(string columnName, Type valueType, SchemaEntryType columnType, int columnLength, NullableType nullable, IRange range = null)
        {
            ColumnName = columnName;
            ValueType = valueType;
            ColumnType = columnType;
            ColumnLength = columnLength;
            ColumnNullable = nullable;
            Range = range;
        }
        public string ColumnName { get; private set; }
        public string LowerColumnName { get { return this.ColumnName.ToLowerInvariant(); } }
        public Type ValueType { get; private set; }
        public SchemaEntryType ColumnType { get; private set; }
        public int ColumnLength { get; private set; }
        public NullableType ColumnNullable { get; private set; }
        public IRange Range { get; private set; }
        public bool HasRange => Range != null;
        public void IsInRange(object value, string dtoName, ValidationIssues issues)
        {
            if (!HasRange) return;
            RangeResult result = Range.IsInRange(value);
            if (result != RangeResult.ValueInRange)
            {
                string insert = (result == RangeResult.ValueBelowMinimum)
                    ? "below the minimum allowed value"
                    : "above the maximum allowed value";
                string message = string.Format("The {0} has {1} which is {2}.", dtoName, ColumnName, insert);
                issues.Add(ValidationIssue.Code.ValueOutOfRange, message);
            }
        }

    }
}
