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
        private const string StepPickerEditorUiAlias = "Umb.PropertyEditorUi.Dropdown.Single";

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

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.SingleDropDown, out IDataEditor? singleDropDownEditor))
            {
                logger.LogError("Could not create the Step Picker, as no {EditorAlias} property editor was found.", Constants.PropertyEditors.Aliases.SingleDropDown);
                return Fail(new InvalidOperationException($"Property editor '{Constants.PropertyEditors.Aliases.SingleDropDown}' was not found."));
            }

            IDictionary<string, object> stepPickerConfiguration = singleDropDownEditor
                .GetConfigurationEditor()
                .FromConfigurationObject(
                    new SingleDropDownConfiguration
                    {
                        Items = stepPickerItems,
                    },
                    configurationEditorJsonSerializer);

            IDataType? currentStepPicker = await dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid);
            if (currentStepPicker == null)
            {
                DataType dataType = CreateDataType(singleDropDownEditor, x =>
                {
                    x.Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid;
                    x.Name = "[Umbraco Commerce Checkout] Step Picker";
                    x.EditorUiAlias = StepPickerEditorUiAlias;
                    x.DatabaseType = ValueStorageType.Nvarchar;
                    x.ConfigurationData = stepPickerConfiguration;
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
                // A Step Picker created before v19 (or restored from a pre-v19 Deploy/uSync artifact) is still on
                // the Flexible dropdown, which yields a collection. Move it across the way the CMS migration does.
                currentStepPicker.Editor = singleDropDownEditor;
                currentStepPicker.EditorUiAlias = StepPickerEditorUiAlias;
                currentStepPicker.ConfigurationData = stepPickerConfiguration;

                Attempt<IDataType, DataTypeOperationStatus> updateAttempt = await dataTypeService.UpdateAsync(currentStepPicker, Constants.Security.SuperUserKey);
                if (!updateAttempt.Success)
                {
                    logger.LogError(updateAttempt.Exception, "Update step picker attempt status {AttemptStatus}.", updateAttempt.Status);
                    return Fail(updateAttempt.Exception);
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
