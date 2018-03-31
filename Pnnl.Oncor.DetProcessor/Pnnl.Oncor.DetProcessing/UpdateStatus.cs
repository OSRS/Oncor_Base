using System.Collections.Generic;

namespace Pnnl.Oncor.DetProcessing
{
    public enum UpdateIssue
    {
        AllOk,
        NoFilePosted,
        NoExistingFile,
        NoExistingEntry,
        FileValidationIssues,
        DataIssue,
        Security,
        SystemIssue
    }

    public sealed class IssueNotice
    {
        public readonly string Title;
        public readonly string Message;

        public IssueNotice(string t, string m)
        {
            this.Title = t;
            this.Message = m;
        }
    }

    public sealed class UpdateStatus
    {
        public readonly UpdateIssue Issue;

        public bool HasIssue
        {
            get { return this.Issue != UpdateIssue.AllOk; }
        }

        private readonly List<IssueNotice> notices = new List<IssueNotice>();
        public IEnumerable<IssueNotice> Notices
        {
            get { return this.notices.AsReadOnly(); }
        }

        public void Add(IssueNotice notice)
        {
            this.notices.Add(notice);
        }

        public int Count
        {
            get { return this.notices.Count; }
        }

        public UpdateStatus(UpdateIssue issue)
        {
            this.Issue = issue;
        }
    }
}
