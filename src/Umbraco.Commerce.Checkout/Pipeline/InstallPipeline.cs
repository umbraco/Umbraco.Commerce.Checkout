using System;
using System.Collections.Generic;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    public class InstallPipeline(Func<IEnumerable<IAsyncPipelineTask<InstallPipelineData>>> items) : PipelineTaskCollection<InstallPipelineData>(items)
    {
    }
}
