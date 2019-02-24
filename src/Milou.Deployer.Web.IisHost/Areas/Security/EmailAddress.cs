using System;
using System.Linq;

namespace Milou.Deployer.Web.IisHost.Areas.Security
{
    public class EmailAddress
    {
        private EmailAddress(string address)
        {
            Address = address;
        }

        public string Address { get; }

        public string Domain => Address.Split('@').Last();

        public static bool TryParse(string email, out EmailAddress emailAddress)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                emailAddress = default;
                return false;
            }

            //TODO add real email parsing
            emailAddress = new EmailAddress(email);
            return email.IndexOf("@", StringComparison.Ordinal) >= 0;
        }
    }
}