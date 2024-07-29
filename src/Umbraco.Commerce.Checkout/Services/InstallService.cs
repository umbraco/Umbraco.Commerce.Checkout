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
            PipelineResult<InstallPipelineContext> result = await PipelineRunner.InvokeAsync<InstallAsyncPipelineTask, InstallPipelineContext>(new InstallPipelineContext
            {
                SiteRootNodeId = siteRootNodeId,
                Store = store,
            });

            if (!result.Success)
            {
                throw result.Exception;
            }
        }
    }
}
