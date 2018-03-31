namespace Osrs.Oncor.DetFactories
{
    public partial interface IRange
    {
        object Maximum { get; }
        object Minimum { get; }
        RangeResult IsInRange(object value);
    }
}