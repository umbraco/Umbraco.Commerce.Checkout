using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutDataTypesTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public CreateUmbracoCommerceCheckoutDataTypesTask(
            IDataTypeService dataTypeService,
            PropertyEditorCollection propertyEditors,
            IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            // Theme Color Picker
            if (_propertyEditors.TryGet(Constants.PropertyEditors.Aliases.ColorPicker, out IDataEditor? colorPickerDataEditor))
            {
                IDataType? currentColorPicker = _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid).GetAwaiter().GetResult();
                if (currentColorPicker == null)
                {
                    DataType dataType = CreateDataType(colorPickerDataEditor, x =>
                    {
                        x.Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid;
                        x.Name = "[Umbraco Commerce Checkout] Theme Color Picker";
                        x.EditorUiAlias = "Umb.PropertyEditorUi.ColorPicker";
                        x.DatabaseType = ValueStorageType.Nvarchar;
                        x.ConfigurationData = colorPickerDataEditor
                            .GetConfigurationEditor()
                            .FromConfigurationObject(
                                new ColorPickerConfiguration
                                {
                                    Items = UmbracoCommerceCheckoutConstants.ColorMap.Select((kvp, idx) => new ColorPickerConfiguration.ColorPickerItem
                                    {
                                        Label = kvp.Value,
                                        Value = kvp.Key,
                                    }).ToList(),
                                    UseLabel = false,
                                },
                                _configurationEditorJsonSerializer);
                    });

                    _dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
                }
                else
                {
                    currentColorPicker.EditorUiAlias = "Umb.PropertyEditorUi.ColorPicker"; // this field is added in cms v14
                    currentColorPicker.ConfigurationData = colorPickerDataEditor
                            .GetConfigurationEditor()
                            .FromConfigurationObject(
                                new ColorPickerConfiguration
                                {
                                    Items = UmbracoCommerceCheckoutConstants.ColorMap.Select((kvp, idx) => new ColorPickerConfiguration.ColorPickerItem
                                    {
                                        Label = kvp.Value,
                                        Value = kvp.Key,
                                    }).ToList(),
                                    UseLabel = false,
                                },
                                _configurationEditorJsonSerializer);

                    _dataTypeService.UpdateAsync(currentColorPicker, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
                }
            }

            // Step Picker
            List<string> stepPickerItems = [
                "Information",
                "ShippingMethod",
                "PaymentMethod",
                "Review",
                "Payment",
                "Confirmation",
            ];

            IDataType? currentStepPicker = _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid).GetAwaiter().GetResult();
            if (_propertyEditors.TryGet(Constants.PropertyEditors.Aliases.DropDownListFlexible, out IDataEditor? ddlDataEditor))
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
                                _configurationEditorJsonSerializer);
                    });

                    _dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
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
                            _configurationEditorJsonSerializer);

                    _dataTypeService.UpdateAsync(currentStepPicker, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
                }
            }

            // Continue the pipeline
            return Ok();
        }

        private DataType CreateDataType(IDataEditor dataEditor, Action<DataType> config)
        {
            var dataType = new DataType(dataEditor, _configurationEditorJsonSerializer);

            config.Invoke(dataType);

            return dataType;
        }
    }
}
