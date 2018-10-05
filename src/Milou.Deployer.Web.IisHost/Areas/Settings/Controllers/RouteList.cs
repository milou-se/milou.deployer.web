using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public static class RouteList
    {
        public static ImmutableArray<RouteInfo> GetConstantRoutes(IReadOnlyCollection<Assembly> assemblies)
        {
            ImmutableArray<RouteInfo> immutableArray = assemblies.SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => type.IsSealed && type.IsAbstract)
                .Select(type => (Type: type,Fields: type.GetFields().Where(field => field.IsLiteral && !field.IsInitOnly && field.IsPublic && field.FieldType == typeof(string) && field.Name.Contains("Route", StringComparison.Ordinal) && !field.Name.EndsWith("Name", StringComparison.Ordinal))))
                .SelectMany(typeFields => typeFields.Fields.Select(field => new RouteInfo(typeFields.Type.FullName, field.Name, field.GetValue(null) as string)))
                .ToImmutableArray();

            return immutableArray;
        }

        public static ImmutableArray<ControllerRouteInfo> GetRoutesWithController(IReadOnlyCollection<Assembly> assemblies)
        {
            ImmutableArray<ControllerActions> controllerActions = assemblies.SelectMany(assembly =>
                    assembly.GetLoadableTypes().Where(type => typeof(Controller).IsAssignableFrom(type)
                                                              && type.IsPublic && !type.IsAbstract))
                .Select(GetControllerActions)
                .ToImmutableArray();

            ImmutableArray<RouteInfo> constantRoutes = GetConstantRoutes(assemblies);

            ImmutableArray<ControllerRouteInfo> controllerRoutes = constantRoutes.Select(route => new ControllerRouteInfo(route, controllerActions.SingleOrDefault(s => s.RouteAttributes.Any(r => r?.Name != null && r.Name.Equals(route.Name, StringComparison.Ordinal)))?.ControllerType.FullName)).ToImmutableArray();

            return controllerRoutes;
        }

        private static ControllerActions GetControllerActions(Type controllerType)
        {
            ImmutableArray<RouteAttribute> routeAttributes = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(method => method.GetCustomAttributes<RouteAttribute>()).ToImmutableArray();

            return new ControllerActions(controllerType, routeAttributes);
        }
    }
}