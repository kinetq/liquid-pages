using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace Kinetq.LiquidPages.Extension.Dialogs;

[DataContract]
internal class AddLiquidErrorPageData : NotifyPropertyChangedObject
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

    private int? _statusCode = null;
    [DataMember]
    public int? StatusCode
    {
        get => _statusCode;
        set => SetProperty(ref _statusCode, value);
    }
}