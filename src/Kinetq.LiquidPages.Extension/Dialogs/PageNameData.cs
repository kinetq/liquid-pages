using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace Kinetq.LiquidPages.Extension.Dialogs;

[DataContract]
internal class PageNameData : NotifyPropertyChangedObject
{
    private readonly TaskCompletionSource<bool> _dialogResultSource = new();

    public PageNameData()
    {
        AcceptCommand = new AsyncCommand((parameter, cancellationToken) =>
        {
            _dialogResultSource.TrySetResult(true);
            return Task.CompletedTask;
        });

        CancelCommand = new AsyncCommand((parameter, cancellationToken) =>
        {
            _dialogResultSource.TrySetResult(false);
            return Task.CompletedTask;
        });
    }

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

    /// <summary>
    /// Gets a task that completes when the dialog is closed.
    /// </summary>
    public Task<bool> DialogResult => this._dialogResultSource.Task;

    /// <summary>
    /// Gets the command to accept the dialog.
    /// </summary>
    [DataMember]
    public IAsyncCommand AcceptCommand { get; }

    /// <summary>
    /// Gets the command to cancel the dialog.
    /// </summary>
    [DataMember]
    public IAsyncCommand CancelCommand { get; }
}