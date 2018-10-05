using System;

namespace Milou.Deployer.Web.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegistrationOrderAttribute : Attribute
    {
        public int Order { get; }

        public string Tag { get; set; }

        public bool ReRegisterEnabled { get; set; }

        public RegistrationOrderAttribute(int order)
        {
            Order = order;
        }
    }
}