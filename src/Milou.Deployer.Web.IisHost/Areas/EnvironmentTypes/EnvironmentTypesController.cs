using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Milou.Deployer.Web.Core;
using Milou.Deployer.Web.IisHost.Controllers;

namespace Milou.Deployer.Web.IisHost.Areas.EnvironmentTypes
{
    public class EnvironmentTypesController : BaseApiController
    {
        public const string HttpGetRouteName = nameof(HttpGetRoute);

        public const string HttpGetRoute = "/environmenttypes";

        private readonly IEnvironmentTypeService _environmentTypes;

        public EnvironmentTypesController(IEnvironmentTypeService environmentTypes) =>
            _environmentTypes = environmentTypes;

        [HttpGet]
        [Route(HttpGetRoute, Name = HttpGetRouteName)]
        public async Task<ObjectResult> Index()
        {
            var environmentTypes = await _environmentTypes.GetEnvironmentTypes();

            return new ObjectResult(new {Types = environmentTypes});
        }
    }
}