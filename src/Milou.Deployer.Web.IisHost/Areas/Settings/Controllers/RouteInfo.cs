using System;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class RouteInfo
    {
        public string Type { get; }

        public string Name { get; }

        public string Value { get; }

        public RouteInfo(string type, string name, string value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        public string RouteName => Name + "Name";

        public bool IsLinkable() => !Value.Contains("{") && !Name.Contains("Post", StringComparison.Ordinal);
    }
}