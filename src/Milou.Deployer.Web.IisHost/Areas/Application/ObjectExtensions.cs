using System;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.IisHost.Areas.Application
{
    public static class ObjectExtensions
    {
        public static T Cast<T>([NotNull] this object instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance is T typedInstance)
            {
                return typedInstance;
            }

            throw new InvalidOperationException(
                $"The instance {instance} could not be converted to type {typeof(T)}");
        }
    }
}
