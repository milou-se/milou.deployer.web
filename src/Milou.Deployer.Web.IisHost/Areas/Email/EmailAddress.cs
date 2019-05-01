using System;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [UsedImplicitly]
    public class EmailAddress : IValidationObject
    {
        public EmailAddress(string address)
        {
            Address = address;
        }

        public string Address { get; }


        public override string ToString()
        {
            return $"{nameof(Address)}: {Address}, {nameof(IsValid)}: {IsValid}";
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Address)
                               && Address.Contains("@", StringComparison.OrdinalIgnoreCase);
    }
}
