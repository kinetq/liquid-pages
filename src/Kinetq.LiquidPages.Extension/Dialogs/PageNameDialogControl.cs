namespace Kinetq.LiquidPages.Extension.Dialogs;

using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

/// <summary>
/// A remote user control to get the page name from the user.
/// </summary>
[DataContract]
internal class PageNameDialogControl : RemoteUserControl
{
    public PageNameDialogControl()
        : base(dataContext: null)
    {
    }

    /// <summary>
    /// Gets or sets the page name entered by the user.
    /// </summary>
    [DataMember]
    public string PageName { get; set; } = "NewPage";

    /// <summary>
    /// Gets or sets whether the dialog was confirmed.
    /// </summary>
    [DataMember]
    public bool IsConfirmed { get; set; }

    /// <summary>
    /// Gets the command to accept the dialog.
    /// </summary>
    public IAsyncCommand AcceptCommand => new AsyncCommand((parameter, cancellationToken) =>
    {
        this.IsConfirmed = true;
        return Task.CompletedTask;
    });

    /// <summary>
    /// Gets the command to cancel the dialog.
    /// </summary>
    public IAsyncCommand CancelCommand => new AsyncCommand((parameter, cancellationToken) =>
    {
        this.IsConfirmed = false;
        return Task.CompletedTask;
    });
}
