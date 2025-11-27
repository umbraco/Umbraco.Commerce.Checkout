using Umbraco.Commerce.Common.Pipelines;

namespace Umbraco.Commerce.Checkout.Pipeline;

public class InstallPipelineArgs(InstallPipelineData model) : PipelineArgs<InstallPipelineData>(model);
