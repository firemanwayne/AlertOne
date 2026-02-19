using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace AlertOne.Core.Abstractions;

/// <summary>
/// Base class for all client-side entities and data models.
/// Provides INotifyPropertyChanged infrastructure and a string Id.
/// </summary>
public abstract class DataModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _id = string.Empty;

    [Display(Name = "ID")]
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}
