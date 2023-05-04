using System;
using System.Collections.Generic;
using Umbraco.Commerce.Common.Pipelines;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    public class InstallPipeline : PipelineTaskCollection<InstallPipelineContext>
    {
        public InstallPipeline(Func<IEnumerable<IPipelineTask<InstallPipelineContext>>> items)
            : base(items)
        { }
    }
}