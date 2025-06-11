using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CreateUmbracoCommerceCheckoutDocumentTypesTask> _logger;

        public CreateUmbracoCommerceCheckoutDocumentTypesTask(
            IContentTypeService contentTypeService,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IContentTypeContainerService contentTypeContainerService,
            ILogger<CreateUmbracoCommerceCheckoutDocumentTypesTask> logger)
        {
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
            _shortStringHelper = shortStringHelper;
            _contentTypeContainerService = contentTypeContainerService;
            _logger = logger;
        }

        public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Begin CreateUmbracoCommerceCheckoutDocumentTypesTask");

            // Setup lazy data types
            var textstringDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid));
            var textareaDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextareaGuid));
            var booleanDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.CheckboxGuid));
            var contentPickerDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.ContentPickerGuid));
            var imagePickerDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(Constants.DataTypes.Guids.MediaPicker3SingleImageGuid));
            var themeColorPickerDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.ThemeColorPickerGuid));
            var stepPickerDataType = new Lazy<Task<IDataType?>>(() => _dataTypeService.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid));

            // Checkout content type folder
            _logger.LogInformation("Create or update checkout document type folder");
            EntityContainer? checkoutContentTypeFolder = await _contentTypeContainerService.GetAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid);
            if (checkoutContentTypeFolder == null)
            {
                _logger.LogInformation("Checkout document type folder is not found, creating a new folder");
                Attempt<EntityContainer?, EntityContainerOperationStatus> folderCreateAttempt = await _contentTypeContainerService.CreateAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.BasePageGuid, "[Umbraco Commerce Checkout] Page", null, Constants.Security.SuperUserKey);
                if (!folderCreateAttempt.Success)
                {
                    throw new InvalidOperationException("Unable to create a folder to store checkout package content types");
                }

                checkoutContentTypeFolder = folderCreateAttempt.Result;
            }

            // Checkout Step Page
            PropertyType[] checkoutStepProps =
            [
                CreatePropertyType(await textstringDataType.Value, x =>
                {
                    x.Alias = "uccShortStepName";
                    x.Name = "Short Step Name";
                    x.Description = "A short name for this step to display in the checkout navigation.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(await stepPickerDataType.Value, x =>
                {
                    x.Alias = "uccStepType";
                    x.Name = "Step Type";
                    x.Description = "The checkout step to display for this step of the checkout flow.";
                    x.SortOrder = 20;
                })
            ];

            _logger.LogInformation("Create or update checkout step page document type");
            IContentType? checkoutStepPageContentType = await _contentTypeService.GetAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid);
            if (checkoutStepPageContentType == null)
            {
                checkoutStepPageContentType = new ContentType(_shortStringHelper, -1)
                {
                    Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutStepPageGuid,
                    Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage,
                    Name = "[Umbraco Commerce Checkout] Checkout Step Page",
                    Icon = "icon-settings-alt color-green",
                    PropertyGroups = new PropertyGroupCollection([
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutStepProps))
                        {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 100,
                        },
                    ]),
                };

                Attempt<ContentTypeOperationStatus> createAttempt = await _contentTypeService.CreateAsync(checkoutStepPageContentType, Constants.Security.SuperUserKey);
                if (!createAttempt.Success)
                {
                    _logger.LogError(createAttempt.Exception, "Create checkout step page document type attempt status {AttemptStatus}.", createAttempt.Result);
                    return Fail(createAttempt.Exception);
                }
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
                    Attempt<ContentTypeOperationStatus> updateAttempt = await _contentTypeService.UpdateAsync(checkoutStepPageContentType, Constants.Security.SuperUserKey);
                    if (!updateAttempt.Success)
                    {
                        _logger.LogError(updateAttempt.Exception, "Update checkout step page document type attempt status {AttemptStatus}.", updateAttempt.Result);
                        return Fail(updateAttempt.Exception);
                    }
                }
            }

            // Move to the dedicated folder
            _logger.LogInformation("Moving checkout step document types to the correct folder.");
            _contentTypeService.MoveAsync(checkoutStepPageContentType.Key, checkoutContentTypeFolder!.Key);

            // Checkout Page
            PropertyType[] checkoutPageProps =
            [
                CreatePropertyType(await imagePickerDataType.Value, x =>
                {
                    x.Alias = "uccStoreLogo";
                    x.Name = "Store Logo";
                    x.Description = "A logo image for the store to appear at the top of the checkout screens and order emails.";
                    x.SortOrder = 10;
                }),
                CreatePropertyType(await textstringDataType.Value, x =>
                {
                    x.Alias = "uccStoreAddress";
                    x.Name = "Store Address";
                    x.Description = "The address of the web store to appear in the footer of order emails.";
                    x.SortOrder = 20;
                }),
                CreatePropertyType(await themeColorPickerDataType.Value, x =>
                {
                    x.Alias = "uccThemeColor";
                    x.Name = "Theme Color";
                    x.Description = "The theme color to use for colored elements of the checkout pages.";
                    x.SortOrder = 30;
                }),
                CreatePropertyType(await booleanDataType.Value, x =>
                {
                    x.Alias = "uccCollectShippingInfo";
                    x.Name = "Collect Shipping Info";
                    x.Description = "Select whether to collect shipping information or not. Not necessary if you are only dealing with digital downloads.";
                    x.SortOrder = 40;
                }),
                CreatePropertyType(await textstringDataType.Value, x =>
                {
                    x.Alias = "uccOrderLinePropertyAliases";
                    x.Name = "Order Line Property Aliases";
                    x.Description = "Comma separated list of order line property aliases to display in the order summary.";
                    x.SortOrder = 50;
                }),
                CreatePropertyType(await contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccBackPage";
                    x.Name = "Checkout Back Page";
                    x.Description = "The page to go back to when backing out of the checkout flow.";
                    x.SortOrder = 60;
                }),
                CreatePropertyType(await contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccTermsAndConditionsPage";
                    x.Name = "Terms and Conditions Page";
                    x.Description = "The page on the site containing the terms and conditions.";
                    x.SortOrder = 70;
                }),
                CreatePropertyType(await contentPickerDataType.Value, x =>
                {
                    x.Alias = "uccPrivacyPolicyPage";
                    x.Name = "Privacy Policy Page";
                    x.Description = "The page on the site containing the privacy policy.";
                    x.SortOrder = 80;
                }),
                CreatePropertyType(await booleanDataType.Value, x =>
                {
                    x.Alias = "umbracoNaviHide";
                    x.Name = "Hide from Navigation";
                    x.Description = "Hide the checkout page from the sites main navigation.";
                    x.SortOrder = 90;
                })
            ];

            _logger.LogInformation("Create or update checkout page document type");
            IContentType? checkoutPageContentType = await _contentTypeService.GetAsync(UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid);
            if (checkoutPageContentType == null)
            {
                checkoutPageContentType = new ContentType(_shortStringHelper, -1)
                {
                    Key = UmbracoCommerceCheckoutConstants.ContentTypes.Guids.CheckoutPageGuid,
                    Alias = UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage,
                    Name = "[Umbraco Commerce Checkout] Checkout Page",
                    Icon = "icon-cash-register color-green",
                    AllowedContentTypes = [
                        new ContentTypeSort(checkoutStepPageContentType.Key, 1, checkoutStepPageContentType.Alias),
                    ],
                    PropertyGroups = new PropertyGroupCollection([
                        new PropertyGroup(new PropertyTypeCollection(true, checkoutPageProps))
                        {
                            Alias = "settings",
                            Name = "Settings",
                            Type = PropertyGroupType.Group,
                            SortOrder = 50,
                        },
                    ]),
                };

                Attempt<ContentTypeOperationStatus> createAttempt = await _contentTypeService.CreateAsync(checkoutPageContentType, Constants.Security.SuperUserKey);
                if (!createAttempt.Success)
                {
                    _logger.LogError(createAttempt.Exception, "Create checkout page attempt status {AttemptStatus}.", createAttempt.Result);
                    return Fail(createAttempt.Exception);
                }
            }
            else
            {
                bool safeExisting = false;
                bool hasSettingsGroup = checkoutPageContentType.PropertyGroups.Contains("Settings");
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
                    Attempt<ContentTypeOperationStatus> updateAttempt = await _contentTypeService.UpdateAsync(checkoutPageContentType, Constants.Security.SuperUserKey);
                    if (!updateAttempt.Success)
                    {
                        _logger.LogError(updateAttempt.Exception, "Update checkout step page document type attempt status {AttemptStatus}.", updateAttempt.Result);
                        return Fail(updateAttempt.Exception);
                    }
                }
            }

            // Move to the dedicated folder
            _contentTypeService.MoveAsync(checkoutPageContentType.Key, checkoutContentTypeFolder!.Key);

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
