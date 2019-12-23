using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Arbor.App.Extensions;
using Arbor.App.Extensions.Application;
using Arbor.App.Extensions.Cli;
using Arbor.App.Extensions.Configuration;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Decorators;
using Arbor.KVConfiguration.JsonConfiguration;
using Arbor.KVConfiguration.UserConfiguration;

namespace Arbor.AspNetCore.Host.Configuration
{
    public static class ConfigurationInitialization
    {
        public static MultiSourceKeyValueConfiguration InitializeStartupConfiguration(IReadOnlyList<string> args, IReadOnlyDictionary<string, string> environmentVariables, IReadOnlyCollection<Assembly> assemblies)
        {
            var multiSourceKeyValueConfiguration = KeyValueConfigurationManager
                .Add(NoConfiguration.Empty)
                .AddReflectionSettings(assemblies)
                .AddEnvironmentVariables(environmentVariables)
                .AddCommandLineArgsSettings(args)
                .DecorateWith(new ExpandKeyValueConfigurationDecorator())
                .Build();

            return multiSourceKeyValueConfiguration;
        }

        private static AppSettingsBuilder AddUserSettings(this AppSettingsBuilder builder, string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return builder;
            }

            return builder.Add(new UserJsonConfiguration(basePath));
        }

        private static AppSettingsBuilder AddLoggingSettings(this AppSettingsBuilder builder)
        {
            var loggingSettings = new NameValueCollection
            {
                { "Logging:LogLevel:Default", "Warning" },
                { "Logging:LogLevel:System.Net.Http.HttpClient", "Warning" },
                { "LogLevel:System.Net.Http.HttpClient", "Warning" }
            };

            var memoryKeyValueConfiguration = new InMemoryKeyValueConfiguration(loggingSettings);
            return builder.Add(memoryKeyValueConfiguration);
        }

        private static AppSettingsBuilder AddReflectionSettings(
            this AppSettingsBuilder appSettingsBuilder,
            IReadOnlyCollection<Assembly> scanAssemblies)
        {
            if (scanAssemblies is null)
            {
                return appSettingsBuilder;
            }

            foreach (var currentAssembly in scanAssemblies.OrderBy(assembly => assembly.FullName))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(
                        new ReflectionKeyValueConfiguration(currentAssembly));
            }

            return appSettingsBuilder;
        }

        private static AppSettingsBuilder AddSettingsFileFromArgsOrEnvironment(
            this AppSettingsBuilder appSettingsBuilder,
            IReadOnlyList<string> args,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            var settingsPath = args?.ParseParameter(ConfigurationConstants.JsonSettingsFile)
                               ?? environmentVariables.ValueOrDefault(ConfigurationConstants.JsonSettingsFile);

            if (settingsPath.HasValue() && File.Exists(settingsPath))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(new JsonKeyValueConfiguration(settingsPath));
            }

            return appSettingsBuilder;
        }

        public static MultiSourceKeyValueConfiguration InitializeConfiguration(
            Func<string, string> basePath = null,
            string contentBasePath = null,
            IReadOnlyCollection<Assembly> scanAssemblies = null,
            IReadOnlyList<string> args = null,
            IReadOnlyDictionary<string, string> environmentVariables = null)
        {
            var multiSourceKeyValueConfiguration = KeyValueConfigurationManager
                .Add(NoConfiguration.Empty)
                .AddReflectionSettings(scanAssemblies)
                .AddLoggingSettings()
                .AddJsonSettings(basePath, args, environmentVariables)
                .AddMachineSpecificSettings(basePath)
                .AddSettingsFileFromArgsOrEnvironment(args, environmentVariables)
                .AddEnvironmentVariables(environmentVariables)
                .AddUserSettings(contentBasePath)
                .AddCommandLineArgsSettings(args)
                .DecorateWith(new ExpandKeyValueConfigurationDecorator()).Build();

            return multiSourceKeyValueConfiguration;
        }

        public static AppSettingsBuilder AddEnvironmentVariables(
            this AppSettingsBuilder builder,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            if (environmentVariables is null)
            {
                return builder;
            }

            var nameValueCollection = new NameValueCollection();
            foreach (var environmentVariable in environmentVariables)
            {
                nameValueCollection.Add(environmentVariable.Key, environmentVariable.Value);
            }

            return builder.Add(new InMemoryKeyValueConfiguration(nameValueCollection));
        }

        public static AppSettingsBuilder AddCommandLineArgsSettings(
            this AppSettingsBuilder builder,
            IReadOnlyList<string> args)
        {
            if (args is null)
            {
                return builder;
            }

            var nameValueCollection = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

            const char variableAssignmentCharacter = '=';

            foreach (var arg in args.Where(a =>
                a.Count(c => c == variableAssignmentCharacter) == 1 && a.Length >= 3))
            {
                var parts = arg.Split(variableAssignmentCharacter, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0];
                var value = parts[1];

                nameValueCollection.Add(key, value);
            }

            var inMemoryKeyValueConfiguration = new InMemoryKeyValueConfiguration(nameValueCollection);
            return builder.Add(inMemoryKeyValueConfiguration);
        }

        public static AppSettingsBuilder AddJsonSettings(
            this AppSettingsBuilder appSettingsBuilder,
            Func<string, string> basePath,
            IReadOnlyCollection<string> args,
            IReadOnlyDictionary<string, string> environmentVariables)


        {
            if (basePath is null)
            {
                return appSettingsBuilder;
            }

            var environmentName = args?.ParseParameter(ApplicationConstants.AspNetEnvironment)
                                  ?? environmentVariables.ValueOrDefault(ApplicationConstants.AspNetEnvironment)
                                  ?? ApplicationConstants.EnvironmentProduction;

            return appSettingsBuilder.Add(new JsonKeyValueConfiguration(basePath("settings.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{environmentName}.json"), false));
        }

        public static AppSettingsBuilder AddMachineSpecificSettings(
            this AppSettingsBuilder appSettingsBuilder,
            Func<string, string> basePath)
        {
            if (basePath is null)
            {
                return appSettingsBuilder;
            }

            FileInfo MachineSpecificConfig(DirectoryInfo directoryInfo)
            {
                return directoryInfo.GetFiles($"settings.{Environment.MachineName}.json").SingleOrDefault();
            }

            string MachineSpecificFile()
            {
                var baseDirectory = new DirectoryInfo(basePath(null));

                FileInfo machineSpecificConfig = null;

                var currentDirectory = baseDirectory;

                while (machineSpecificConfig is null && currentDirectory != null)
                {
                    try
                    {
                        machineSpecificConfig = MachineSpecificConfig(currentDirectory);

                        currentDirectory = currentDirectory.Parent;
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        return null;
                    }
                }

                return machineSpecificConfig?.FullName;
            }

            var machineSpecificFile = MachineSpecificFile();

            if (!string.IsNullOrWhiteSpace(machineSpecificFile))
            {
                appSettingsBuilder = appSettingsBuilder.Add(new JsonKeyValueConfiguration(machineSpecificFile));
            }

            return appSettingsBuilder;
        }
    }
}
