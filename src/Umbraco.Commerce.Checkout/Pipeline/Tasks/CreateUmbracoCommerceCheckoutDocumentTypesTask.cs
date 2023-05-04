using System;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutDocumentTypesTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;

        private readonly IShortStringHelper _shortStringHelper;

        public CreateUmbracoCommerceCheckoutDocumentTypesTask(IContentTypeService contentTypeService, IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper)
        {
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _shortStringHelper = shortStringHelper;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            // Setup variables
            IContentType existing;

            int checkoutPageContentTypeId;
            int checkoutStepPageContentTypeId;

            // Setup lazy data types
            var textstringDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(Constants.DataTypes.Guids.TextstringGuid));
            var textareaDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(Constants.DataTypes.Guids.TextareaGuid));
            var booleanDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(Constants.DataTypes.Guids.CheckboxGuid));
            var contentPickerDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(Constants.DataTypes.Guids.ContentPickerGuid));
            var imagePickerDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(Constants.DataTypes.Guids.MediaPicker3SingleImageGuid));
            var themeColorPickerDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid));
            var stepPickerDataType = new Lazy<IDataType>(() => _dataTypeService.GetDataType(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid));

            // Checkout Base Page
            existing = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid);
            
            if (existing == null)
            {
                var contentType = CreateContentType(-1, x =>
                {
                    x.Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid;
                    x.Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.BasePage;
                    x.Name = "[Umbraco Commerce Checkout] Page";
                });

                _contentTypeService.Save(contentType);

                checkoutPageContentTypeId = contentType.Id;
            }
            else
            {
                checkoutPageContentTypeId = existing.Id;
            }

            // Checkout Step Page
            var checkoutStepProps = new[]{
                CreatePropertyType(textstringDataType.Value, x => {
                    x.Alias = "uccShortStepName";
                    x.Name = "Short Step Name";
                    x.Description = "A short name for this step to display in the checkout navigation.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(stepPickerDataType.Value, x => {
                    x.Alias = "uccStepType";
                    x.Name = "Step Type";
                    x.Description = "The checkout step to display for this step of the checkout flow.";
                    x.SortOrder = 20;
                })
            };

            existing = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid);
            
            if (existing == null)
            {
                var contentType = CreateContentType(checkoutPageContentTypeId, x => 
                {
                    x.Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid;
                    x.Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage;
                    x.Name = "[Umbraco Commerce Checkout] Checkout Step Page";
                    x.Icon = "icon-settings-alt color-green";
                    x.PropertyGroups = new PropertyGroupCollection(new[]{
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutStepProps)) {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder =100
                        }
                    });
                });

                _contentTypeService.Save(contentType);

                checkoutStepPageContentTypeId = contentType.Id;
            }
            else
            {
                var safeExisting = false;
                var hasSettingsGroup = existing.PropertyGroups.Contains("Settings");
                var settingsGroup = hasSettingsGroup
                    ? existing.PropertyGroups["Settings"]
                    : new PropertyGroup(new PropertyTypeCollection(true, checkoutStepProps)) 
                    {
                        Alias = "settings",
                        Name = "Settings",
                        Type = PropertyGroupType.Group,
                        SortOrder =100
                    };

                foreach (var prop in checkoutStepProps)
                {
                    if (!settingsGroup.PropertyTypes.Contains(prop.Alias))
                    {
                        settingsGroup.PropertyTypes.Add(prop);
                        safeExisting = true;
                    }
                }

                if (!hasSettingsGroup)
                {
                    existing.PropertyGroups.Add(settingsGroup);
                    safeExisting = true;
                }

                if (safeExisting)
                {
                    _contentTypeService.Save(existing);
                }

                checkoutStepPageContentTypeId = existing.Id;
            }

            // Checkout Page
            var checkoutPageProps = new[]{
                CreatePropertyType(imagePickerDataType.Value, x => {
                    x.Alias = "uccStoreLogo";
                    x.Name = "Store Logo";
                    x.Description = "A logo image for the store to appear at the top of the checkout screens and order emails.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(textstringDataType.Value, x => {
                    x.Alias = "uccStoreAddress";
                    x.Name = "Store Address";
                    x.Description = "The address of the web store to appear in the footer of order emails.";
                    x.SortOrder = 20;
                }),
                CreatePropertyType(themeColorPickerDataType.Value, x => {
                    x.Alias = "uccThemeColor";
                    x.Name = "Theme Color";
                    x.Description = "The theme color to use for colored elements of the checkout pages.";
                    x.SortOrder = 30;
                }),
                CreatePropertyType(booleanDataType.Value, x => {
                    x.Alias = "uccCollectShippingInfo";
                    x.Name = "Collect Shipping Info";
                    x.Description = "Select whether to collect shipping information or not. Not necessary if you are only dealing with digital downloads.";
                    x.SortOrder = 40;
                }),
                CreatePropertyType(textstringDataType.Value, x => {
                    x.Alias = "uccOrderLinePropertyAliases";
                    x.Name = "Order Line Property Aliases";
                    x.Description = "Comma separated list of order line property aliases to display in the order summary.";
                    x.SortOrder = 50;
                }),
                CreatePropertyType(contentPickerDataType.Value, x => {
                    x.Alias = "uccBackPage";
                    x.Name = "Checkout Back Page";
                    x.Description = "The page to go back to when backing out of the checkout flow.";
                    x.SortOrder = 60;
                }),
                CreatePropertyType(contentPickerDataType.Value, x => {
                    x.Alias = "uccTermsAndConditionsPage";
                    x.Name = "Terms and Conditions Page";
                    x.Description = "The page on the site containing the terms and conditions.";
                    x.SortOrder = 70;
                }),
                CreatePropertyType(contentPickerDataType.Value, x => {
                    x.Alias = "uccPrivacyPolicyPage";
                    x.Name = "Privacy Policy Page";
                    x.Description = "The page on the site containing the privacy policy.";
                    x.SortOrder = 80;
                }),
                CreatePropertyType(booleanDataType.Value, x => {
                    x.Alias = "umbracoNaviHide";
                    x.Name = "Hide from Navigation";
                    x.Description = "Hide the checkout page from the sites main navigation.";
                    x.SortOrder = 90;
                })
            };

            existing = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid);

            if (existing == null)
            {
                var contentType = CreateContentType(checkoutPageContentTypeId, x =>
                {
                    x.Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid;
                    x.Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage;
                    x.Name = "[Umbraco Commerce Checkout] Checkout Page";
                    x.Icon = "icon-cash-register color-green";
                    x.AllowedContentTypes = new[]{
                        new ContentTypeSort(checkoutStepPageContentTypeId, 1)
                    };
                    x.PropertyGroups = new PropertyGroupCollection(new[]{
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutPageProps)) {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 50
                        }
                    });
                });

                _contentTypeService.Save(contentType);
            }
            else
            {
                var safeExisting = false;
                var hasSettingsGroup = existing.PropertyGroups.Contains("Settings");
                var settingsGroup = hasSettingsGroup
                    ? existing.PropertyGroups["Settings"]
                    : new PropertyGroup(new PropertyTypeCollection(true, checkoutPageProps))
                    {
                        Alias = "settings",
                        Name = "Settings",
                        Type = PropertyGroupType.Group,
                        SortOrder = 100
                    };

                foreach (var prop in checkoutPageProps)
                {
                    if (!settingsGroup.PropertyTypes.Contains(prop.Alias))
                    {
                        settingsGroup.PropertyTypes.Add(prop);
                        safeExisting = true;
                    }
                }

                if (!hasSettingsGroup)
                {
                    existing.PropertyGroups.Add(settingsGroup);
                    safeExisting = true;
                }

                if (safeExisting)
                {
                    _contentTypeService.Save(existing);
                }
            }

            // Continue the pipeline
            return Ok();
        }

        private ContentType CreateContentType(int parentId, Action<ContentType> config)
        {
            var contentType = new ContentType(_shortStringHelper, parentId);

            config.Invoke(contentType);

            return contentType;
        }

        private PropertyType CreatePropertyType(IDataType dataType, Action<PropertyType> config)
        {
            var propertyType = new PropertyType(_shortStringHelper, dataType);

            config.Invoke(propertyType);

            return propertyType;
        }
    }
}
