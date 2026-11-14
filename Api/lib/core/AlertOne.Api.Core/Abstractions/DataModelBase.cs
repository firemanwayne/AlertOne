namespace AlertOne.Api.Core.Abstractions;

/// <summary>
/// Base class for all client-side entities and data models.
/// Provides INotifyPropertyChanged infrastructure and a string Id.
/// </summary>
public abstract class DataModelBase
{
    /// <summary>
    /// Backing field for <see cref="Id"/>.
    /// </summary>
    private string _id = string.Empty;
    /// <summary>
    /// Backing field for <see cref="CreatedBy"/>.
    /// </summary>
    private string _createdBy = string.Empty;
    /// <summary>
    /// Backing field for <see cref="UpdatedBy"/>.
    /// </summary>
    private string _updatedBy = string.Empty;
    /// <summary>
    /// Backing field for <see cref="CreatedDate"/>.
    /// </summary>
    private DateTime _createdDate = DateTime.UtcNow;
    /// <summary>
    /// Backing field for <see cref="UpdatedDate"/>.
    /// </summary>
    private DateTime _updatedDate = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier of the record.
    /// </summary>
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// Gets or sets the UTC date and time the record was created.
    /// </summary>
    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetProperty(ref _createdDate, value);
    }

    /// <summary>
    /// Gets or sets the identifier of the user that created the record.
    /// </summary>
    public string CreatedBy
    {
        get => _createdBy;
        set => SetProperty(ref _createdBy, value);
    }

    /// <summary>
    /// Gets or sets the UTC date and time the record was last updated.
    /// </summary>
    public DateTime UpdatedDate
    {
        get => _updatedDate;
        set => SetProperty(ref _updatedDate, value);
    }

    /// <summary>
    /// Gets or sets the identifier of the user that last updated the record.
    /// </summary>
    public string UpdatedBy
    {
        get => _updatedBy;
        set => SetProperty(ref _updatedBy, value);
    }

    /// <summary>
    /// Sets a backing field when the value changed and returns whether a change was made.
    /// </summary>
    protected static bool SetProperty<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;

        return true;
    }
}
