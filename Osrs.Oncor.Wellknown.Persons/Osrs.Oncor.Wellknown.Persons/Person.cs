using Osrs.Data;
using Osrs.Runtime;
using System;

namespace Osrs.Oncor.Wellknown.Persons
{
    public sealed class Person : IIdentifiableEntity<CompoundIdentity>, IEquatable<Person>
    {
        //NOTE -- load and modify contact values via the mutable item --  this.Contacts.Add("work", EmailAddress.TryCreate("foo.roo@pnnl.gov"));
        private readonly SimpleContactInfo contacts = new SimpleContactInfo();
        public SimpleContactInfo Contacts
        {
            get { return this.contacts; }
        }

        public CompoundIdentity Identity
        {
            get;
        }

        private string firstName;
        public string FirstName
        {
            get { return this.firstName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.firstName = value;
            }
        }

        private string lastName;
        public string LastName
        {
            get { return this.lastName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.lastName = value;
            }
        }

        public string Name
        {
            get
            {
                return this.firstName + " " + this.lastName;
            }
        }

        public Person(CompoundIdentity id, string firstName, string lastName)
        {
            MethodContract.NotNullOrEmpty(id, nameof(id));
            MethodContract.NotNullOrEmpty(firstName, nameof(firstName));
            MethodContract.NotNullOrEmpty(lastName, nameof(lastName));
            this.Identity = id;
            this.firstName = firstName;
            this.lastName = lastName;            
        }

        public bool Equals(IIdentifiableEntity<CompoundIdentity> other)
        {
            return this.Equals(other as Person);
        }

        public bool Equals(Person other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
