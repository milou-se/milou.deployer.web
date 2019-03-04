using System;
using System.Linq;
using Newtonsoft.Json;

namespace Milou.Deployer.Web.IisHost.Areas.Deployment.Controllers
{
    public class DeploymentLogViewOutputModel
    {
        public DeploymentLogViewOutputModel(string log)
        {
            var pattern = @"{""MessageTemplate"":";

            var items = new
            {
                items =
                    log
                        .Split(new[] { pattern }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => JsonConvert.DeserializeObject(pattern + d)).ToArray()
            };

            Log = JsonConvert.SerializeObject(items);
        }

        public string Log { get; }
    }
}
