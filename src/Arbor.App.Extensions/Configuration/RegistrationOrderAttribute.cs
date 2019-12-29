using System;

namespace Arbor.App.Extensions.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegistrationOrderAttribute : Attribute
    {
        public RegistrationOrderAttribute(int order) => Order = order;

        public int Order { get; }

        public string? Tag { get; set; }

        public bool RegisterInRootScope { get; set; }
    }
}