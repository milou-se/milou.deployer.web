using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.IisHost.Areas.Email
{
    [UsedImplicitly]
    public class Email : IValidationObject
    {
        public Email(string address)
        {
            Address = address;
        }

        public string Address { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Address)
                               && Address.Contains("@");


        public override string ToString()
        {
            return $"{nameof(Address)}: {Address}, {nameof(IsValid)}: {IsValid}";
        }
    }
}