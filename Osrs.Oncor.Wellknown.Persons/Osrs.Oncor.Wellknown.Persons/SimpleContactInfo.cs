using Osrs.Runtime;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Osrs.Oncor.Wellknown.Persons
{
    public sealed class SimpleContactInfo
    {
        private Dictionary<string, EmailAddress> emails = new Dictionary<string, EmailAddress>();

        public bool Exists(string name)
        {
            if (!string.IsNullOrEmpty(name))
                return this.emails.ContainsKey(name);
            return false;
        }

        public bool Exists(EmailAddress address)
        {
            if (address!=null)
            {
                foreach(EmailAddress cur in this.emails.Values)
                {
                    if (address.Equals(cur))
                        return true;
                }
            }
            return false;
        }

        public void Add(string name, EmailAddress address)
        {
            if (!string.IsNullOrEmpty(name) && address!=null)
            {
                this.emails[name] = address;
            }
        }

        public void Remove(string name)
        {
            if (!string.IsNullOrEmpty(name) && this.emails.ContainsKey(name))
                this.emails.Remove(name);
        }

        public EmailAddress Get(string name)
        {
            if (!string.IsNullOrEmpty(name) && this.emails.ContainsKey(name))
                return this.emails[name];
            return null;
        }

		public Dictionary<string, EmailAddress> Get()
		{
			return emails;
		}
    }

    public sealed class EmailAddress : IEquatable<EmailAddress>, IComparable<EmailAddress>
    {
        public string AddressText
        {
            get;
        }

        public string Recipient
        {
            get { return this.AddressText.Substring(0, this.AddressText.IndexOf('@') - 1); }
        }

        public string Domain
        {
            get { return this.AddressText.Substring(this.AddressText.IndexOf('@')); }
        }

        public EmailAddress(string address)
        {
            MethodContract.Assert(IsValidEmailAddress(address), nameof(address));
            this.AddressText = address;
        }

        public static EmailAddress TryCreate(string address)
        {
            if (IsValidEmailAddress(address))
                return new EmailAddress(address);
            return null;
        }

        public static bool IsValidEmailAddress(string strIn)
        {
            if (!string.IsNullOrEmpty(strIn))
            {
                try
                {
                    return Regex.IsMatch(strIn,
                          @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                          @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                          RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                }
                catch (RegexMatchTimeoutException)
                { }
            }
            return false;
        }

        public bool Equals(EmailAddress other, bool justDomain)
        {
            if (justDomain)
                return this.Equals(other);
            if (other != null)
                return this.Domain.Equals(other.Domain, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public bool Equals(EmailAddress other)
        {
            if (other != null)
                return this.AddressText.Equals(other.AddressText, StringComparison.OrdinalIgnoreCase);

            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EmailAddress);
        }

        public override int GetHashCode()
        {
            return this.AddressText.GetHashCode();
        }

        public override string ToString()
        {
            return this.AddressText;
        }

        public int CompareTo(EmailAddress other)
        {
            return this.AddressText.CompareTo(other.AddressText);
        }
    }
}
