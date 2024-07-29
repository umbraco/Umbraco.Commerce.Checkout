using System;
using System.Collections.Generic;
using Umbraco.Commerce.Common.Pipelines;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    public class InstallAsyncPipelineTask : AsyncPipelineTaskCollection<InstallPipelineContext>
    {
        public InstallAsyncPipelineTask(Func<IEnumerable<IAsyncPipelineTask<InstallPipelineContext>>> items)
            : base(items)
        {
        }
    }
}
