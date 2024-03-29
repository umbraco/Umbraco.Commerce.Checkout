using Umbraco.Commerce.Checkout.Pipeline;
using Umbraco.Commerce.Core.Models;
using PipelineRunner = Umbraco.Commerce.Common.Pipelines.Pipeline;

namespace Umbraco.Commerce.Checkout.Services
{
    public class InstallService
    {
        public void Install(int siteRootNodeId, StoreReadOnly store)
        {
            var result = PipelineRunner.Invoke<InstallPipeline, InstallPipelineContext>(new InstallPipelineContext
            {
                SiteRootNodeId = siteRootNodeId,
                Store = store
            });

            if (!result.Success)
                throw result.Exception;
        }
    }
}
