using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories
{
    public class ValidationIssues : IEnumerable<ValidationIssue>
    {
        private readonly List<ValidationIssue> _issueList = new List<ValidationIssue>();

        public int Count => _issueList.Count;

        public bool IsValid => Count == 0;

        public void Add(ValidationIssue.Code issueCode, string issueMessage)
        {
            ValidationIssue issue = new ValidationIssue(issueCode, issueMessage);
            _issueList.Add(issue);
        }

        public void Merge(ValidationIssues newIssues)
        {
            if (newIssues!=null)
                _issueList.AddRange(newIssues);
        }

        public IEnumerator<ValidationIssue> GetEnumerator()
        {
            return _issueList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ValidationIssue item)
        {
            if (item != null)
                _issueList.Add(item);
        }

        public ValidationIssue Collapse()
        {
            return Collapse(null);
        }
        public ValidationIssue Collapse(string name)
        {
            if (_issueList.Count>0)
            {
                if (_issueList.Count == 1)
                {
                    if (string.IsNullOrEmpty(name))
                        return _issueList[0];
                    else
                        return new ValidationIssue(_issueList[0].IssueCode, name + ": " + _issueList[0].IssueMessage);
                }
                if (string.IsNullOrEmpty(name))
                    return new ValidationIssue(_issueList[0].IssueCode, "Mulitple (" + _issueList.Count + ") issues found");
                else
                    return new ValidationIssue(_issueList[0].IssueCode, name + ": " + "Mulitple (" + _issueList.Count + ") issues found");
            }
            return null;
        }
    }
}
