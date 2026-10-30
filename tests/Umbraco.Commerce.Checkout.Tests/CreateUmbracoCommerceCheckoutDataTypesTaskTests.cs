using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Commerce.Checkout.Pipeline;
using Umbraco.Commerce.Checkout.Pipeline.Tasks;

namespace Umbraco.Commerce.Checkout.Tests;

public class CreateUmbracoCommerceCheckoutDataTypesTaskTests
{
    private const string SingleEditorUiAlias = "Umb.PropertyEditorUi.Dropdown.Single";
    private static readonly string[] _expectedItems =
    [
        "Information",
        "ShippingMethod",
        "PaymentMethod",
        "Review",
        "Payment",
        "Confirmation",
    ];

    private readonly Mock<IDataTypeService> _dataTypeService = new();
    private readonly IConfigurationEditorJsonSerializer _serializer =
        new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
    private readonly SingleDropDownPropertyEditor _singleEditor;
    private readonly DropDownFlexiblePropertyEditor _flexibleEditor;
    private readonly CreateUmbracoCommerceCheckoutDataTypesTask _task;
    private IDataType? _created;
    private IDataType? _updated;

    public CreateUmbracoCommerceCheckoutDataTypesTaskTests()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>();
        var ioHelper = Mock.Of<IIOHelper>();
        _singleEditor = new SingleDropDownPropertyEditor(dataValueEditorFactory, ioHelper, _serializer);
        _flexibleEditor = new DropDownFlexiblePropertyEditor(dataValueEditorFactory, ioHelper, _serializer);

        var propertyEditors = new PropertyEditorCollection(
            new DataEditorCollection(() => [_singleEditor, _flexibleEditor]));

        _dataTypeService
            .Setup(x => x.CreateAsync(It.IsAny<IDataType>(), It.IsAny<Guid>()))
            .Callback((IDataType dt, Guid _) => _created = dt)
            .ReturnsAsync((IDataType dt, Guid _) => Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, dt));
        _dataTypeService
            .Setup(x => x.UpdateAsync(It.IsAny<IDataType>(), It.IsAny<Guid>()))
            .Callback((IDataType dt, Guid _) => _updated = dt)
            .ReturnsAsync((IDataType dt, Guid _) => Attempt.SucceedWithStatus(DataTypeOperationStatus.Success, dt));

        _task = new CreateUmbracoCommerceCheckoutDataTypesTask(
            Mock.Of<ILogger<CreateUmbracoCommerceCheckoutDataTypesTask>>(),
            _dataTypeService.Object,
            propertyEditors,
            _serializer);
    }

    [Fact]
    public async Task FreshInstall_Creates_Step_Picker_On_Single_Dropdown_With_Items()
    {
        SetupExisting(null);

        await _task.ExecuteAsync(new InstallPipelineArgs(new InstallPipelineData()), CancellationToken.None);

        Assert.NotNull(_created);
        Assert.Null(_updated);
        Assert.Equal(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid, _created.Key);
        Assert.Equal(Constants.PropertyEditors.Aliases.SingleDropDown, _created.EditorAlias);
        Assert.Equal(SingleEditorUiAlias, _created.EditorUiAlias);
        AssertConfiguration(_created);
    }

    [Fact]
    public async Task Existing_Flexible_Step_Picker_Is_Moved_To_Single_Dropdown()
    {
        var existing = new DataType(_flexibleEditor, _serializer)
        {
            Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid,
            Name = "[Umbraco Commerce Checkout] Step Picker",
            EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
            ConfigurationData = new Dictionary<string, object>
            {
                ["items"] = new[] { "Information" },
                ["multiple"] = false,
            },
        };
        SetupExisting(existing);

        await _task.ExecuteAsync(new InstallPipelineArgs(new InstallPipelineData()), CancellationToken.None);

        Assert.Null(_created);
        Assert.Same(existing, _updated);
        Assert.Equal(Constants.PropertyEditors.Aliases.SingleDropDown, existing.EditorAlias);
        Assert.Equal(SingleEditorUiAlias, existing.EditorUiAlias);
        Assert.False(existing.ConfigurationData.ContainsKey("multiple"));
        AssertConfiguration(existing);
    }

    [Fact]
    public async Task Existing_Single_Step_Picker_Is_Updated_Without_Changing_Editor()
    {
        var existing = new DataType(_singleEditor, _serializer)
        {
            Key = UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid,
            Name = "[Umbraco Commerce Checkout] Step Picker",
            EditorUiAlias = "Umb.PropertyEditorUi.Dropdown.Single",
            ConfigurationData = new Dictionary<string, object> { ["items"] = new[] { "Information" } },
        };
        SetupExisting(existing);

        await _task.ExecuteAsync(new InstallPipelineArgs(new InstallPipelineData()), CancellationToken.None);

        Assert.Null(_created);
        Assert.Same(existing, _updated);
        Assert.Same(_singleEditor, existing.Editor);
        Assert.Equal(SingleEditorUiAlias, existing.EditorUiAlias);
        AssertConfiguration(existing);
    }

    private void SetupExisting(IDataType? existing) =>
        _dataTypeService
            .Setup(x => x.GetAsync(UmbracoCommerceCheckoutConstants.DataTypes.Guids.StepPickerGuid))
            .ReturnsAsync(existing);

    private static void AssertConfiguration(IDataType dataType)
    {
        var configuration = Assert.IsType<SingleDropDownConfiguration>(dataType.ConfigurationObject);
        Assert.Equal(_expectedItems, configuration.Items);
    }
}
