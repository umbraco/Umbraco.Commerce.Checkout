using System;
using System.Collections.Generic;
using Umbraco.Commerce.Common.Pipelines;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    public class InstallAsyncPipelineTask(Func<IEnumerable<IAsyncPipelineTask<InstallPipelineContext>>> items) : PipelineTaskCollection<InstallPipelineContext>(items)
    {
    }
}
