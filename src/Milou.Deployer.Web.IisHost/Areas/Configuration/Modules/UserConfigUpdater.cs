using System;
using System.Collections.Immutable;
using System.IO;
using Arbor.KVConfiguration.JsonConfiguration;
using Arbor.KVConfiguration.Urns;
using JetBrains.Annotations;
using Milou.Deployer.Web.Core.Validation;

namespace Milou.Deployer.Web.IisHost.Areas.Configuration.Modules
{
    [UsedImplicitly]
    public class UserConfigUpdater
    {
        private readonly ConfigurationHolder _configurationHolder;
        private string _fileName;
        private FileSystemWatcher _fileSystemWatcher;

        public UserConfigUpdater(
            ConfigurationHolder configurationHolder)
        {
            _configurationHolder = configurationHolder;

            _fileName = Path.Combine(Directory.GetCurrentDirectory(), "config.user");

            if (File.Exists(_fileName))
            {
                var fileInfo = new FileInfo(_fileName);

                if (fileInfo.Directory != null)
                {
                    _fileSystemWatcher = new FileSystemWatcher(fileInfo.Directory.FullName, fileInfo.Name);
                    _fileSystemWatcher.Changed += WatcherOnChanged;
                    _fileSystemWatcher.Created += WatcherOnChanged;
                    _fileSystemWatcher.Renamed += WatcherOnChanged;
                }
            }
        }

        public void Start()
        {
            if (File.Exists(_fileName))
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            ImmutableArray<Type> types = _configurationHolder.RegisteredTypes;

            var jsonKeyValueConfiguration = new JsonKeyValueConfiguration(_fileName);

            foreach (Type type in types)
            {
                ImmutableArray<INamedInstance<object>> allInstances = jsonKeyValueConfiguration.GetNamedInstances(type);

                foreach (INamedInstance<object> instance in allInstances)
                {
                    if (instance.Value is IValidationObject validationObject)
                    {
                        if (validationObject.IsValid)
                        {
                            _configurationHolder.Add(instance);
                        }
                    }
                }
            }
        }
    }
}
