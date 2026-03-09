using System.Runtime.CompilerServices;

namespace AlertOne.Core.Abstractions;

/// <summary>
/// Base class for all client-side entities and data models.
/// Provides INotifyPropertyChanged infrastructure and a string Id.
/// </summary>
public abstract class DataModelBase
{
    private string _id = Guid.NewGuid().ToString();
    private string _createdBy = string.Empty;
    private string _updatedBy = string.Empty;
    private DateTime _createdDate = DateTime.UtcNow;
    private DateTime _updatedDate = DateTime.UtcNow;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetProperty(ref _createdDate, value);
    }

    public string CreatedBy
    {
        get => _createdBy;
        set => SetProperty(ref _createdBy, value);
    }

    public DateTime UpdatedDate
    {
        get => _updatedDate;
        set => SetProperty(ref _updatedDate, value);
    }

    public string UpdatedBy
    {
        get => _updatedBy;
        set => SetProperty(ref _updatedBy, value);
    }

#pragma warning disable CA1822 // Mark members as static
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
#pragma warning restore CA1822 // Mark members as static
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;

        return true;
    }
}
