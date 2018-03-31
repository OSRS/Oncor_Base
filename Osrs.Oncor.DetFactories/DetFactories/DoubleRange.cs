using System;

namespace Osrs.Oncor.DetFactories
{
    public class DoubleRange : IRange
    {
        public object Maximum { get; }
        public object Minimum { get; }

        public DoubleRange(double? min, double? max)
        {
            Maximum = max;
            Minimum = min;
        }

        public RangeResult IsInRange(object value)
        {
            double? doubleValue = (double?) value;
            RangeResult result = RangeResult.ValueInRange;
            double? doubleMinimum = (double?) Minimum;
            if (doubleValue < doubleMinimum)
            {
                result = RangeResult.ValueBelowMinimum;
            }
            double? doubleMaximum = (double?) Maximum;
            if (doubleValue > doubleMaximum)
            {
                result = RangeResult.ValueAboveMaximum;
            }
            return result;
        }
    }
}
