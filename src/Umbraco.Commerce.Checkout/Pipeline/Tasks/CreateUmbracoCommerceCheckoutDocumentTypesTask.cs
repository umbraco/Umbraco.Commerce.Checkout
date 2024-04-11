using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutDocumentTypesTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IContentTypeContainerService _contentTypeContainerService;

        public CreateUmbracoCommerceCheckoutDocumentTypesTask(
            IContentTypeService contentTypeService,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IContentTypeContainerService contentTypeContainerService)
        {
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _shortStringHelper = shortStringHelper;
            _contentTypeContainerService = contentTypeContainerService;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            // Setup lazy data types
            var textstringDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid).GetAwaiter().GetResult());
            var textareaDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextareaGuid).GetAwaiter().GetResult());
            var booleanDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.CheckboxGuid).GetAwaiter().GetResult());
            var contentPickerDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.ContentPickerGuid).GetAwaiter().GetResult());
            var imagePickerDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.MediaPicker3SingleImageGuid).GetAwaiter().GetResult());
            var themeColorPickerDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid).GetAwaiter().GetResult());
            var stepPickerDataType = new Lazy<IDataType?>(() => _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid).GetAwaiter().GetResult());

            // Checkout Base Page
            //IContentType? checkoutContentTypeFolder = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid);
            //Attempt<OperationResult<OperationResultType, EntityContainer>?> folderCreateAttempt = _contentTypeService.CreateContainer(-1, UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid, "[Umbraco Commerce Checkout] Page", Constants.Security.SuperUserId);
            //if (!folderCreateAttempt.Success)
            //{
            //    throw new InvalidOperationException("Unable to create a folder to store checkout package content types");
            //}

            EntityContainer? checkoutContentTypeFolder = _contentTypeContainerService.GetAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid).GetAwaiter().GetResult();
            if (checkoutContentTypeFolder == null)
            {
                Attempt<EntityContainer?, EntityContainerOperationStatus> folderCreateAttempt = _contentTypeContainerService.CreateAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid, "[Umbraco Commerce Checkout] Page", null, Constants.Security.SuperUserKey).GetAwaiter().GetResult();
                if (!folderCreateAttempt.Success)
                {
                    throw new InvalidOperationException("Unable to create a folder to store checkout package content types");
                }

                checkoutContentTypeFolder = folderCreateAttempt.Result;
            }


            //if (checkoutContentTypeFolder == null)
            //{
            //    checkoutContentTypeFolder = new ContentType(_shortStringHelper, -1)
            //    {
            //        Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid,
            //        Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.BasePage,
            //        Name = "[Umbraco Commerce Checkout] Page",
            //    };

            //    _contentTypeService.Save(checkoutContentTypeFolder);
            //}


            // Checkout Step Page
            PropertyType[] checkoutStepProps =
            [
                CreatePropertyType(textstringDataType.Value, x =>
                {
                    x.Alias = "uccShortStepName";
                    x.Name = "Short Step Name";
                    x.Description = "A short name for this step to display in the checkout navigation.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(stepPickerDataType.Value, x =>
                {
                    x.Alias = "uccStepType";
                    x.Name = "Step Type";
                    x.Description = "The checkout step to display for this step of the checkout flow.";
                    x.SortOrder = 20;
                })
            ];

            IContentType? checkoutStepPageContentType = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid);
            if (checkoutStepPageContentType == null)
            {
                checkoutStepPageContentType = new ContentType(_shortStringHelper, -1)
                {
                    Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid,
                    Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage,
                    Name = "[Umbraco Commerce Checkout] Checkout Step Page",
                    Icon = "icon-settings-alt color-green",
                    PropertyGroups = new PropertyGroupCollection(new[]
                    {
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutStepProps))
                        {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 100,
                        },
                    }),
                };

                _contentTypeService.Save(checkoutStepPageContentType);
            }
            else
            {
                bool safeExisting = false;
                bool hasSettingsGroup = checkoutStepPageContentType.PropertyGroups.Contains("Settings");
                PropertyGroup settingsGroup = hasSettingsGroup
                    ? checkoutStepPageContentType.PropertyGroups["Settings"]
                    : new PropertyGroup(new PropertyTypeCollection(true, checkoutStepProps))
                    {
                        Alias = "settings",
                        Name = "Settings",
                        Type = PropertyGroupType.Group,
                        SortOrder = 100
                    };

                foreach (PropertyType prop in checkoutStepProps)
                {
                    if (settingsGroup.PropertyTypes != null && !settingsGroup.PropertyTypes.Contains(prop.Alias))
                    {
                        settingsGroup.PropertyTypes.Add(prop);
                        safeExisting = true;
                    }
                }

                if (!hasSettingsGroup)
                {
                    checkoutStepPageContentType.PropertyGroups.Add(settingsGroup);
                    safeExisting = true;
                }

                if (safeExisting)
                {
                    _contentTypeService.Save(checkoutStepPageContentType);
                }
            }

            // Move to the dedicated folder
            _ = _contentTypeService.MoveAsync(checkoutStepPageContentType.Key, checkoutContentTypeFolder!.Key).GetAwaiter().GetResult();

            // Checkout Page
            PropertyType[] checkoutPageProps =
            [
                CreatePropertyType(imagePickerDataType.Value, x =>
                {
                    x.Alias = "uccStoreLogo";
                    x.Name = "Store Logo";
                    x.Description = "A logo image for the store to appear at the top of the checkout screens and order emails.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(textstringDataType.Value, x =>
                {
                    x.Alias = "uccStoreAddress";
                    x.Name = "Store Address";
                    x.Description = "The address of the web store to appear in the footer of order emails.";
                    x.SortOrder = 20;
                }),
                CreatePropertyType(themeColorPickerDataType.Value, x =>
                {
                    x.Alias = "uccThemeColor";
                    x.Name = "Theme Color";
                    x.Description = "The theme color to use for colored elements of the checkout pages.";
                    x.SortOrder = 30;
                }),
                CreatePropertyType(booleanDataType.Value, x =>
                {
                    x.Alias = "uccCollectShippingInfo";
                    x.Name = "Collect Shipping Info";
                    x.Description = "Select whether to collect shipping information or not. Not necessary if you are only dealing with digital downloads.";
                    x.SortOrder = 40;
                }),
                CreatePropertyType(textstringDataType.Value, x =>
                {
                    x.Alias = "uccOrderLinePropertyAliases";
                    x.Name = "Order Line Property Aliases";
                    x.Description = "Comma separated list of order line property aliases to display in the order summary.";
                    x.SortOrder = 50;
                }),
                CreatePropertyType(contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccBackPage";
                    x.Name = "Checkout Back Page";
                    x.Description = "The page to go back to when backing out of the checkout flow.";
                    x.SortOrder = 60;
                }),
                CreatePropertyType(contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccTermsAndConditionsPage";
                    x.Name = "Terms and Conditions Page";
                    x.Description = "The page on the site containing the terms and conditions.";
                    x.SortOrder = 70;
                }),
                CreatePropertyType(contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccPrivacyPolicyPage";
                    x.Name = "Privacy Policy Page";
                    x.Description = "The page on the site containing the privacy policy.";
                    x.SortOrder = 80;
                }),
                CreatePropertyType(booleanDataType.Value, x =>
                {
                    x.Alias = "umbracoNaviHide";
                    x.Name = "Hide from Navigation";
                    x.Description = "Hide the checkout page from the sites main navigation.";
                    x.SortOrder = 90;
                })
            ];

            IContentType? checkoutPageContentType = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid);
            if (checkoutPageContentType == null)
            {
                checkoutPageContentType = new ContentType(_shortStringHelper, -1)
                {
                    Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid,
                    Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage,
                    Name = "[Umbraco Commerce Checkout] Checkout Page",
                    Icon = "icon-cash-register color-green",
                    AllowedContentTypes = new[]
                    {
                        new ContentTypeSort(checkoutStepPageContentType.Key, 1, checkoutStepPageContentType.Alias),
                    },
                    PropertyGroups = new PropertyGroupCollection(new[]
                    {
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutPageProps))
                        {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 50,
                        },
                    }),
                };

                _contentTypeService.Save(checkoutPageContentType);
            }
            else
            {
                var safeExisting = false;
                var hasSettingsGroup = checkoutPageContentType.PropertyGroups.Contains("Settings");
                PropertyGroup settingsGroup = hasSettingsGroup
                    ? checkoutPageContentType.PropertyGroups["Settings"]
                    : new PropertyGroup(new PropertyTypeCollection(true, checkoutPageProps))
                    {
                        Alias = "settings",
                        Name = "Settings",
                        Type = PropertyGroupType.Group,
                        SortOrder = 100,
                    };

                foreach (PropertyType prop in checkoutPageProps)
                {
                    if (settingsGroup.PropertyTypes != null && !settingsGroup.PropertyTypes.Contains(prop.Alias))
                    {
                        settingsGroup.PropertyTypes.Add(prop);
                        safeExisting = true;
                    }
                }

                if (!hasSettingsGroup)
                {
                    checkoutPageContentType.PropertyGroups.Add(settingsGroup);
                    safeExisting = true;
                }

                if (safeExisting)
                {
                    _contentTypeService.Save(checkoutPageContentType);
                }
            }

            // Move to the dedicated folder
            _ = _contentTypeService.MoveAsync(checkoutPageContentType.Key, checkoutContentTypeFolder!.Key).GetAwaiter().GetResult();

            // Continue the pipeline
            return Ok();
        }

        private PropertyType CreatePropertyType(IDataType? dataType, Action<PropertyType> config)
        {
            ArgumentNullException.ThrowIfNull(dataType);

            PropertyType propertyType = new(_shortStringHelper, dataType);

            config.Invoke(propertyType);

            return propertyType;
        }
    }
}
