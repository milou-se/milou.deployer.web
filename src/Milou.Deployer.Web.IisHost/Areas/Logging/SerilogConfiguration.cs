using System;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.Core.Extensions;

namespace Milou.Deployer.Web.IisHost.Areas.Logging
{
    [Urn(LoggingConstants.SerilogBaseUrn)]
    [UsedImplicitly]
    public class SerilogConfiguration : IValidationObject
    {
        public SerilogConfiguration(
            string seqUrl,
            string startupLogFilePath,
            string rollingLogFilePath,
            bool seqEnabled = false,
            bool rollingLogFilePathEnabled = false,
            bool consoleEnabled = true)
        {
            if (Uri.TryCreate(seqUrl, UriKind.Absolute, out Uri url))
            {
                SeqUrl = url;
            }

            StartupLogFilePath = startupLogFilePath;
            RollingLogFilePath = rollingLogFilePath;
            SeqEnabled = seqEnabled;
            RollingLogFilePathEnabled = rollingLogFilePathEnabled;
            ConsoleEnabled = consoleEnabled;
        }

        public bool SeqEnabled { get; }

        public bool RollingLogFilePathEnabled { get; }

        public bool ConsoleEnabled { get; }

        public Uri SeqUrl { get; }

        public string StartupLogFilePath { get; }

        public string RollingLogFilePath { get; }

        public bool IsValid => SeqUrl.HasValue();
    }
}