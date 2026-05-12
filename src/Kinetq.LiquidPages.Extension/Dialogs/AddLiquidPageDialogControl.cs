namespace Kinetq.LiquidPages.Extension.Dialogs;

using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

/// <summary>
/// A remote user control to get the page name from the user.
/// </summary>
[DataContract]
internal class AddLiquidPageDialogControl : RemoteUserControl
{
    public AddLiquidPageDialogControl()
        : base(dataContext: new AddLiquidPageData())
    {
    }
}
