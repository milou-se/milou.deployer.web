using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public class ControllerActions
    {
        public ControllerActions(Type controllerType, ImmutableArray<RouteAttribute> routeAttributes)
        {
            ControllerType = controllerType;
            RouteAttributes = routeAttributes;
        }

        public Type ControllerType { get; }
        public ImmutableArray<RouteAttribute> RouteAttributes { get; }
    }
}
