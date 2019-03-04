using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Extensions
{
    public static class UrlExtensions
    {
        [PublicAPI]
        public static string CreateQueryWithQuestionMark([NotNull] IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return $"?{CreateQueryWithoutQuestionMark(parameters)}";
        }

        [PublicAPI]
        public static string CreateQueryWithoutQuestionMark(
            [NotNull] IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var query =
                $"{string.Join("&", parameters.Select(parameter => $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"))}";

            return query;
        }

        [PublicAPI]
        public static Uri WithQueryFromParameters(
            [NotNull] this Uri uri,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var builder = new UriBuilder(uri)
            {
                Query = CreateQueryWithoutQuestionMark(parameters)
            };

            return builder.Uri;
        }
    }
}
