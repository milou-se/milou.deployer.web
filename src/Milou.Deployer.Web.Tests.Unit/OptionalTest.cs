using System.Reflection;
using Milou.Deployer.Web.Core.Configuration;
using Xunit;

namespace Milou.Deployer.Web.Tests.Unit
{
    public class OptionalTest
    {
        [Fact]
        public void ExcludedAutoRegistrationTypeHasOptionalAttribute()
        {
            var optionalAttribute = typeof(ExcludedAutoRegistrationType).GetCustomAttribute<OptionalAttribute>();

            Assert.NotNull(optionalAttribute);
        }
    }
}