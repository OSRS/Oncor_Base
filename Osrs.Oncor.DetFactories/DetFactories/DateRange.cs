using System;

namespace Osrs.Oncor.DetFactories
{
    public class DateRange : IRange
    {
        public object Maximum { get; }
        public object Minimum { get; }
        public DateRange(DateTime? min, DateTime? max)
        {
            Maximum = max;
            Minimum = min;
        }

        public RangeResult IsInRange(object value)
        {
            DateTime? dateValue = (DateTime?)value;
            RangeResult result = RangeResult.ValueInRange;
            DateTime? dateMinimum = (DateTime?) Minimum;
            if (dateValue < dateMinimum)
            {
                result = RangeResult.ValueBelowMinimum;
            }
            DateTime? dateMaximum = (DateTime?) Maximum;
            if (dateValue > dateMaximum)
            {
                result = RangeResult.ValueAboveMaximum;
            }
            return result;
        }
    }
}
