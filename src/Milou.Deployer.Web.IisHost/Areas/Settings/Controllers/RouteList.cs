using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using TypeExtensions = Milou.Deployer.Web.Core.Extensions.TypeExtensions;

namespace Milou.Deployer.Web.IisHost.Areas.Settings.Controllers
{
    public static class RouteList
    {
        public static ImmutableArray<(string, string, string)> GetConstantRoutes(IReadOnlyCollection<Assembly> assemblies)
        {
            ImmutableArray<(string FullName, string Name, string Value)> immutableArray = assemblies.SelectMany(assembly => TypeExtensions.GetLoadableTypes(assembly))
                .Where(type => type.IsSealed && type.IsAbstract)
                .Select(type => (Type: type,Fields: type.GetFields().Where(field => field.IsLiteral && !field.IsInitOnly && field.IsPublic && field.FieldType == typeof(string) && field.Name.Contains("route", StringComparison.OrdinalIgnoreCase) && !field.Name.EndsWith("Name", StringComparison.Ordinal))))
                .SelectMany(typeFields => typeFields.Fields.Select(field => (typeFields.Type.FullName, field.Name, Value: field.GetValue(null) as string)))
                .ToImmutableArray();

            return immutableArray;
        }
    }
}