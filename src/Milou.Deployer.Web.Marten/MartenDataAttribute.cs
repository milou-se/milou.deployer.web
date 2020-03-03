using System;

namespace Milou.Deployer.Web.Marten
{
    [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple =false, Inherited = false)]
    public sealed class MartenDataAttribute : Attribute
    {

    }
}