using System;
using System.IO;
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
        public static MultiSourceKeyValueConfiguration InitializeConfiguration([NotNull] Func<string, string> basePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            string environmentBasedSettingsPath =
                Environment.GetEnvironmentVariable(ConfigurationConstants.JsonSettingsFile);

            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            AppSettingsBuilder appSettingsBuilder = KeyValueConfigurationManager
                .Add(new ReflectionKeyValueConfiguration(typeof(ConfigurationInitialization).Assembly))
                .Add(new JsonKeyValueConfiguration(basePath("settings.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{environmentName}.json"), false))
                .Add(new JsonKeyValueConfiguration(basePath($"settings.{Environment.MachineName}.json"), false));

            if (environmentBasedSettingsPath.HasValue() && File.Exists(environmentBasedSettingsPath))
            {
                appSettingsBuilder =
                    appSettingsBuilder.Add(new JsonKeyValueConfiguration(environmentBasedSettingsPath,
                        true));

                Log.Logger.Information("Added environment based configuration from key '{Key}', file '{File}'", ConfigurationConstants.JsonSettingsFile, environmentBasedSettingsPath);
            }

            string userConfigurationFile = basePath("config.user");

            Log.Logger.Debug("User configuration file is '{UserFile}', exists: {Exists}", userConfigurationFile, File.Exists(userConfigurationFile));

            MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration = appSettingsBuilder
                .Add(new EnvironmentVariableKeyValueConfigurationSource())
                .Add(new JsonKeyValueConfiguration(userConfigurationFile, false))
                .DecorateWith(new ExpandKeyValueConfigurationDecorator())
                .Build();

            Log.Logger.Information("Configuration done {Configuration} using chain {Chain}",
                multiSourceKeyValueConfiguration.ConfigurationItems,
                multiSourceKeyValueConfiguration.SourceChain);

            return multiSourceKeyValueConfiguration;
        }
    }
}