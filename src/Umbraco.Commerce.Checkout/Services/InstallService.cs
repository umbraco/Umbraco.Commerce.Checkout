using System.Threading.Tasks;
using Umbraco.Commerce.Checkout.Pipeline;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Core.Models;
using PipelineRunner = Umbraco.Commerce.Common.Pipelines.Pipeline;

namespace Umbraco.Commerce.Checkout.Services
{
    public class InstallService
    {
        public async Task InstallAsync(int siteRootNodeId, StoreReadOnly store)
        {
            var args = new InstallPipelineArgs(new InstallPipelineData
            {
                SiteRootNodeId = siteRootNodeId,
                Store = store,
            });

            PipelineResult<InstallPipelineData> result = await PipelineRunner.ExecuteAsync<InstallPipeline, InstallPipelineArgs, InstallPipelineData>(args);

            if (!result.Success)
            {
                throw result.Exception;
            }
        }
    }
}
