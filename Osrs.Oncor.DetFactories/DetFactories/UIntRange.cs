namespace Osrs.Oncor.DetFactories
{
    public class UIntRange : IRange
    {
        public object Maximum { get; }
        public object Minimum { get; }

        public UIntRange(uint? min, uint? max)
        {
            Maximum = max;
            Minimum = min;
        }
        public RangeResult IsInRange(object value)
        {
            uint? uintValue = (uint?)value;
            RangeResult result = RangeResult.ValueInRange;
            uint? uintMinimum = (uint?)Minimum;
            if (uintValue < uintMinimum)
            {
                result = RangeResult.ValueBelowMinimum;
            }
            uint? uintMaximum = (uint?)Maximum;
            if (uintValue > uintMaximum)
            {
                result = RangeResult.ValueAboveMaximum;
            }
            return result;
        }
    }
}
