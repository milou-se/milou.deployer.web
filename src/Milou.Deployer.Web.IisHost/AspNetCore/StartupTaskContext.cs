using System.Collections.Generic;
using System.Linq;
using Milou.Deployer.Web.IisHost.Areas.Application;

namespace Milou.Deployer.Web.IisHost.AspNetCore
{
    public class StartupTaskContext
    {
        private readonly IEnumerable<IStartupTask> _startupTasks;

        public StartupTaskContext(IEnumerable<IStartupTask> startupTasks)
        {
            _startupTasks = startupTasks;
        }

        public bool IsCompleted => _startupTasks.All(t => t.IsCompleted);
    }

}
