using System.Collections.Generic;
using System.Linq;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutDataTypesTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public CreateUmbracoCommerceCheckoutDataTypesTask(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors,
            IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            // Theme Color Picker
            var currentColorPicker = _dataTypeService.GetDataType(UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid);
            if (currentColorPicker == null)
            {
                if (_propertyEditors.TryGet(Constants.PropertyEditors.Aliases.ColorPicker, out IDataEditor editor))
                {
                    var dataType = CreateDataType(editor, x => 
                    {
                        x.Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid;
                        x.Name = "[Umbraco Commerce Checkout] Theme Color Picker";
                        x.DatabaseType = ValueStorageType.Nvarchar;
                        x.Configuration = new ColorPickerConfiguration
                        {
                            Items = UmbracoCommerceCheckoutConstants.ColorMap.Select((kvp, idx) => new ValueListConfiguration.ValueListItem
                            {
                                Id = idx,
                                Value = "{\"value\":\"" + kvp.Key + "\", \"label\":\"" + kvp.Value + "\"}"
                            }).ToList(),
                            UseLabel = false
                        };
                    });

                    _dataTypeService.Save(dataType);
                }
            }
            else
            {
                currentColorPicker.Configuration = new ColorPickerConfiguration
                {
                    Items = UmbracoCommerceCheckoutConstants.ColorMap.Select((kvp, idx) => new ValueListConfiguration.ValueListItem
                    {
                        Id = idx,
                        Value = "{\"value\":\"" + kvp.Key + "\", \"label\":\"" + kvp.Value + "\"}"
                    }).ToList(),
                    UseLabel = false
                };

                _dataTypeService.Save(currentColorPicker);
            }

            // Step Picker
            var stepPickerItems = new List<ValueListConfiguration.ValueListItem>
            {
                new ValueListConfiguration.ValueListItem { Id = 1, Value = "Information" },
                new ValueListConfiguration.ValueListItem { Id = 2, Value = "ShippingMethod" },
                new ValueListConfiguration.ValueListItem { Id = 3, Value = "PaymentMethod" },
                new ValueListConfiguration.ValueListItem { Id = 4, Value = "Review" },
                new ValueListConfiguration.ValueListItem { Id = 5, Value = "Payment" },
                new ValueListConfiguration.ValueListItem { Id = 6, Value = "Confirmation" }
            };

            var currentStepPicker = _dataTypeService.GetDataType(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid);
            if (currentStepPicker == null)
            {
                if (_propertyEditors.TryGet(Constants.PropertyEditors.Aliases.DropDownListFlexible, out IDataEditor editor))
                {
                    var dataType = CreateDataType(editor, x =>
                    {
                        x.Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid;
                        x.Name = "[Umbraco Commerce Checkout] Step Picker";
                        x.DatabaseType = ValueStorageType.Nvarchar;
                        x.Configuration = new DropDownFlexibleConfiguration
                        {
                            Items = stepPickerItems,
                            Multiple = false
                        };
                    });

                    _dataTypeService.Save(dataType);
                }
            }
            else
            {
                currentStepPicker.Configuration = new DropDownFlexibleConfiguration
                {
                    Items = stepPickerItems,
                    Multiple = false
                };

                _dataTypeService.Save(currentStepPicker);
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
