using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Decorators;
using Arbor.KVConfiguration.JsonConfiguration;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public static class ConfigurationInitialization
    {
        public static MultiSourceKeyValueConfiguration InitializeConfiguration(
            IReadOnlyList<string> args,
            [NotNull] Func<string, string> basePath,
            ILogger logger,
            IReadOnlyCollection<Assembly> scanAssemblies,
            string contentBasePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            string environmentBasedSettingsPath =
                Environment.GetEnvironmentVariable(ConfigurationConstants.JsonSettingsFile);

            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            AppSettingsBuilder appSettingsBuilder = KeyValueConfigurationManager
                .Add(new InMemoryKeyValueConfiguration(new NameValueCollection()));

            foreach (Assembly currentAssembly in scanAssemblies.OrderBy(assembly => assembly.FullName))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(
                        new ReflectionKeyValueConfiguration(currentAssembly));
            }

            var loggingSettings = new NameValueCollection
            {
                { "Logging:LogLevel:Default", "Warning" },
                { "Logging:LogLevel:System.Net.Http.HttpClient", "Warning" },
                { "LogLevel:System.Net.Http.HttpClient", "Warning" }
            };


            FileInfo MachineSpecificConfig(DirectoryInfo directoryInfo)
            {
                return directoryInfo.GetFiles($"settings.{Environment.MachineName}.json").SingleOrDefault();
            }

            string MachineSpecificFile()
            {
                var baseDirectory = new DirectoryInfo(basePath(null));

                FileInfo machineSpecificConfig = null;

                DirectoryInfo currentDirectory = baseDirectory;

                while (machineSpecificConfig is null && currentDirectory != null)
                {
                    try
                    {
                        machineSpecificConfig = MachineSpecificConfig(currentDirectory);

                        currentDirectory = currentDirectory.Parent;
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        logger.Warning(ex, "Could not find machine specific config file in any parent directory starting with base directory {BaseDirectory}", baseDirectory.FullName);
                        return null;
                    }
                }

                return machineSpecificConfig?.FullName;
            }

            appSettingsBuilder = appSettingsBuilder
                .Add(new InMemoryKeyValueConfiguration(loggingSettings))
                .Add(new JsonKeyValueConfiguration(basePath("settings.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{environmentName}.json"), false));

            string machineSpecificFile = MachineSpecificFile();

            if (!string.IsNullOrWhiteSpace(machineSpecificFile))
            {
                appSettingsBuilder = appSettingsBuilder.Add(new JsonKeyValueConfiguration(machineSpecificFile));
            }

            if (environmentBasedSettingsPath.HasValue() && File.Exists(environmentBasedSettingsPath))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(new JsonKeyValueConfiguration(environmentBasedSettingsPath,
                        true));

                logger.Information("Added environment based configuration from key '{Key}', file '{File}'",
                    ConfigurationConstants.JsonSettingsFile,
                    environmentBasedSettingsPath);
            }

            var nameValueCollection = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

            const char variableAssignmentCharacter = '=';

            foreach (string arg in args.Where(a => a.Count(c => c == variableAssignmentCharacter) == 1 && a.Length >= 3))
            {
                string[] parts = arg.Split(variableAssignmentCharacter, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    Console.WriteLine($"arg {arg} has length {parts.Length}");
                    continue;
                }

                string key = parts[0];
                string value = parts[1];

                nameValueCollection.Add(key, value);
            }

            var inMemoryKeyValueConfiguration = new InMemoryKeyValueConfiguration(nameValueCollection);

            MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration = appSettingsBuilder
                .Add(new JsonKeyValueConfiguration(Path.Combine(contentBasePath, "config.user"), throwWhenNotExists: false))
                .Add(new EnvironmentVariableKeyValueConfigurationSource())
                .Add(inMemoryKeyValueConfiguration)
                .DecorateWith(new ExpandKeyValueConfigurationDecorator())
                .Build();

            logger.Information("Configuration done using chain {Chain}",
                multiSourceKeyValueConfiguration.SourceChain);

            return multiSourceKeyValueConfiguration;
        }
    }
}
