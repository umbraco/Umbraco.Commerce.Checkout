using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    [Obsolete("Use InstallPipeline instead. Will be removed in v18.")]
    public class InstallAsyncPipelineTask(Func<IEnumerable<IAsyncPipelineTask<InstallPipelineContext>>> items)
        : InstallPipeline(() => items().Select(x => new PipelineTaskAdapter(x)))
    {
        private class PipelineTaskAdapter(IAsyncPipelineTask<InstallPipelineContext> innerTask) : PipelineTaskBase<InstallPipelineData>
        {
            public override async Task<PipelineResult<InstallPipelineData>> ExecuteAsync(PipelineArgs<InstallPipelineData> args, CancellationToken cancellationToken)
            {
                var newArgs = new InstallPipelineArgs(args.Model);
                PipelineResult? result = await innerTask.ExecuteAsync(newArgs, cancellationToken);
                return result.Success
                    ? Ok(args.Model)
                    : Fail(args.Model, result.Exception);
            }
        }
    }

    public class InstallPipeline(Func<IEnumerable<IAsyncPipelineTask<InstallPipelineData>>> items) : PipelineTaskCollection<InstallPipelineData>(items)
    {
    }
}
