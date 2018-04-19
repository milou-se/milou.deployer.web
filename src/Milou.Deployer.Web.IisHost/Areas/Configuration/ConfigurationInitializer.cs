using System;
using System.IO;
using Arbor.KVConfiguration.Core;
using Arbor.KVConfiguration.Core.Decorators;
using Arbor.KVConfiguration.JsonConfiguration;
using Arbor.KVConfiguration.UserConfiguration;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Configuration;
using Serilog;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration
{
    public static class ConfigurationInitializer
    {
        private static readonly object _MutexLock = new object();
        private static bool _isinitialied;

        public static MultiSourceKeyValueConfiguration EnsureInitialized([NotNull] DirectoryInfo applicationBasePath)
        {
            if (applicationBasePath == null)
            {
                throw new ArgumentNullException(nameof(applicationBasePath));
            }

            if (_isinitialied)
            {
                return StaticKeyValueConfigurationManager.AppSettings as MultiSourceKeyValueConfiguration;
            }

            lock (_MutexLock)
            {
                if (_isinitialied)
                {
                    return StaticKeyValueConfigurationManager.AppSettings as MultiSourceKeyValueConfiguration;
                }

                MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration = InitializeConfiguration(applicationBasePath);

                _isinitialied = true;

                return multiSourceKeyValueConfiguration;
            }

        }

        private static MultiSourceKeyValueConfiguration InitializeConfiguration([NotNull] DirectoryInfo applicationBasePath)
        {
            if (applicationBasePath == null)
            {
                throw new ArgumentNullException(nameof(applicationBasePath));
            }

            MultiSourceKeyValueConfiguration multiSourceKeyValueConfiguration;
            try
            {
                string settingsFileFullPath =
                    Environment.GetEnvironmentVariable(ConfigurationConstants.SettingsPath);

                string computerName = Environment.GetEnvironmentVariable("COMPUTERNAME");

                AppSettingsBuilder appSettingsBuilder = KeyValueConfigurationManager
                    .Add(new ReflectionKeyValueConfiguration(typeof(ConfigurationInitializer).Assembly))
                    .Add(new ReflectionKeyValueConfiguration(typeof(ConfigurationConstants).Assembly))
                    .Add(new JsonKeyValueConfiguration(Path.Combine(applicationBasePath.FullName, "settings.json"),
                        false))
                    .Add(new JsonKeyValueConfiguration(Path.Combine(applicationBasePath.FullName, $"settings.{computerName}.json"),
                        false));

                if (!string.IsNullOrWhiteSpace(settingsFileFullPath))
                {
                    appSettingsBuilder = appSettingsBuilder
                        .Add(new JsonKeyValueConfiguration(settingsFileFullPath, false));
                }

                multiSourceKeyValueConfiguration = appSettingsBuilder
                    .Add(new UserConfiguration())
                    .DecorateWith(new ExpandKeyValueConfigurationDecorator())
                    .Build();

                StaticKeyValueConfigurationManager.Initialize(multiSourceKeyValueConfiguration);
            }
            catch (Exception ex) when (LogError(ex))
            {
                throw;
            }

            return multiSourceKeyValueConfiguration;
        }

        private static bool LogError(Exception exception)
        {
            Log.Logger.Error(exception, "Could not initialize configuration");

            return false;
        }
    }
}