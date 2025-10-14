using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutDataTypesTask(
        ILogger<CreateUmbracoCommerceCheckoutDataTypesTask> logger,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : PipelineTaskWithTypedArgsBase<InstallPipelineArgs, InstallPipelineData>
    {
        public override async Task<PipelineResult<InstallPipelineData>> ExecuteAsync(InstallPipelineArgs args, CancellationToken cancellationToken)
        {
            // Step Picker
            List<string> stepPickerItems = [
                "Information",
                "ShippingMethod",
                "PaymentMethod",
                "Review",
                "Payment",
                "Confirmation",
            ];

            IDataType? currentStepPicker = await dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid);
            if (propertyEditors.TryGet(Constants.PropertyEditors.Aliases.DropDownListFlexible, out IDataEditor? ddlDataEditor))
            {
                if (currentStepPicker == null)
                {
                    DataType dataType = CreateDataType(ddlDataEditor, x =>
                    {
                        x.Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid;
                        x.Name = "[Umbraco Commerce Checkout] Step Picker";
                        x.EditorUiAlias = "Umb.PropertyEditorUi.Dropdown";
                        x.DatabaseType = ValueStorageType.Nvarchar;
                        x.ConfigurationData = ddlDataEditor
                            .GetConfigurationEditor()
                            .FromConfigurationObject(
                                new DropDownFlexibleConfiguration
                                {
                                    Items = stepPickerItems,
                                    Multiple = false,
                                },
                                configurationEditorJsonSerializer);
                    });

                    Attempt<IDataType, DataTypeOperationStatus> createAttempt = await dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
                    if (!createAttempt.Success)
                    {
                        logger.LogError(createAttempt.Exception, "Create Step Picker attempt status {AttemptStatus}.", createAttempt.Status);
                        return Fail(createAttempt.Exception);
                    }
                }
                else
                {
                    currentStepPicker.EditorUiAlias = "Umb.PropertyEditorUi.Dropdown"; // this field is added in cms v14
                    currentStepPicker.ConfigurationData = ddlDataEditor
                        .GetConfigurationEditor()
                        .FromConfigurationObject(
                            new DropDownFlexibleConfiguration
                            {
                                Items = stepPickerItems,
                                Multiple = false,
                            },
                            configurationEditorJsonSerializer);

                    Attempt<IDataType, DataTypeOperationStatus> updateAttempt = await dataTypeService.UpdateAsync(currentStepPicker, Constants.Security.SuperUserKey);
                    if (!updateAttempt.Success)
                    {
                        logger.LogError(updateAttempt.Exception, "Update step picker attempt status {AttemptStatus}.", updateAttempt.Status);
                        return Fail(updateAttempt.Exception);
                    }
                }
            }

            // Continue the pipeline
            return Ok();
        }

        private DataType CreateDataType(IDataEditor dataEditor, Action<DataType> config)
        {
            var dataType = new DataType(dataEditor, configurationEditorJsonSerializer);

            config.Invoke(dataType);

            return dataType;
        }
    }
}
