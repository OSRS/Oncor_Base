namespace Osrs.Oncor.DetFactories
{
    public interface IValidatable
    {
        ValidationIssues ValidationIssues { get; }
        void Validate();
    }
}
