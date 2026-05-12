using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace Kinetq.LiquidPages.Extension.Dialogs;

[DataContract]
internal class PageNameData : NotifyPropertyChangedObject
{
    private string? _pageName = string.Empty;
    [DataMember]
    public string? PageName
    {
        get => _pageName;
        set => SetProperty(ref _pageName, value);
    }

    private bool? _force = false;
    [DataMember] public bool? Force 
    {
        get => _force;
        set => SetProperty(ref _force, value);
    }

    private bool? _embeddedResourceConfig = false;
    [DataMember]
    public bool? EmbeddedResourceConfig
    {
        get => _embeddedResourceConfig;
        set => SetProperty(ref _embeddedResourceConfig, value);
    }

    private bool? _generateLayout = false;
    [DataMember]
    public bool? GenerateLayout
    {
        get => _generateLayout;
        set => SetProperty(ref _generateLayout, value);
    }
}