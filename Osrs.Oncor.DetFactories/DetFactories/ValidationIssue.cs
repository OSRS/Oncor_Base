namespace Osrs.Oncor.DetFactories
{
    public class ValidationIssue
    {
        public enum Code
        {
            NoDataCode,
            DuplicateHeader,
            MissingFieldHeader,
            NonUniqueKeyCode,
            MissingUniqueKeyCode,
            MissingForeignKeyCode,
            MissingRequiredFieldCode,
            MissinOptionalFieldsCode,
            StringTooLongCode,
            ValueOutOfRange,
            ValueInvalidFormat,
            MissingDataTab,
            InvalidForeignKeyCode,
            TemporalConsistencyCode,
        }
        public Code IssueCode { get; }
        public string IssueMessage { get; }

        public ValidationIssue(Code issueCode, string issueMessage)
        {
            IssueCode = issueCode;
            IssueMessage = issueMessage;
        }
    }
}
