namespace AlertOne.Core.Abstractions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Serialization;

/// <summary>
/// Base class for view models that edit an entity, with change notification, spinner state and audit fields.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Constructor based off a new entity.
    /// </summary>
    /// <param name="user"></param>
    protected ViewModelBase(ClaimsPrincipal user)
    {
        Id = Guid.NewGuid().ToString();
        IsNew = true;
        CurrentUser = user;
        UpdatedBy = user.GetUserId();
    }

    /// <summary>
    /// Constructor based off an existing entity.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    protected ViewModelBase(string id, ClaimsPrincipal user)
    {
        Id = id;
        IsNew = false;
        CurrentUser = user;
    }

    /// <summary>
    /// Id of the entity. BASE CLASS PROPERTY.
    /// </summary>
    [Required, Display(Name = "Id")]
    public string Id { get; set; }

    /// <summary>
    /// DateTime model instance was created. BASE CLASS PROPERTY.
    /// </summary>
    [Required, Display(Name = "Created Date")]
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// User who created the model instance. BASE CLASS PROPERTY.
    /// </summary>
    [StringLength(100), Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// DateTime model instance was updated. BASE CLASS PROPERTY.
    /// </summary>
    [Required, Display(Name = "Updated Date")]
    public DateTime UpdatedDate { get; init; }

    /// <summary>
    /// User who updated the model instance. BASE CLASS PROPERTY.
    /// </summary>
    [StringLength(100), Display(Name = "Updated By")]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Convenience property to determine if the instance 
    /// is a new model or an existing model
    /// </summary>
    [Display(Name = "Is New?")]
    public bool IsNew { get; set; }

    /// <summary>
    /// Gets a value indicating whether the busy spinner is visible.
    /// </summary>
    public bool SpinnerVisible { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the submit button is disabled.
    /// </summary>
    public bool DisableSpinnerButton { get; set; }

    /// <summary>
    /// Gets the submit button text: "Submit" for new entities, "Update" for existing ones.
    /// </summary>
    public string AddEditButtonText => IsNew ? "Submit" : "Update";

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for a property.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    /// <summary>
    /// Shows the spinner and disables the submit button.
    /// </summary>
    public void ShowSpinnerAndDisableButton()
    {
        SpinnerVisible = true;
        DisableSpinnerButton = true;

        OnPropertyChanged();
    }

    /// <summary>
    /// Hides the spinner and enables the submit button.
    /// </summary>
    public void HideSpinnerAndEnableButton()
    {
        SpinnerVisible = false;
        DisableSpinnerButton = false;

        OnPropertyChanged();
    }

    /// <summary>
    /// Convenience property to set the url segment of index page
    /// </summary>
    public string IndexSegment { get; protected set; } = string.Empty;

    [JsonIgnore] public ClaimsPrincipal CurrentUser { get; set; }

    /// <summary>
    /// Gets a hash code from the identifier and last update time.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Id, UpdatedDate);

    /// <summary>
    /// Sets a field and raises <see cref="PropertyChanged"/> when the value changed; returns whether it changed.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
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

/// <summary>
/// Base class for view models bound to a typed data model.
/// </summary>
public abstract class ViewModelBase<T> : ViewModelBase where T : DataModelBase
{
    /// <summary>
    /// Constructor based off a new entity
    /// </summary>
    /// <param name="user"></param>
    protected ViewModelBase(ClaimsPrincipal user) : base(user)
    {
        string? userId = user.GetUserId();

        UpdatedBy = userId ?? string.Empty;

        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor based off an existing entity
    /// </summary>
    /// <param name="e"></param>
    /// <param name="user"></param>
    protected ViewModelBase(T e, ClaimsPrincipal user) : base(e.Id, user)
    {
        string? userId = user.GetUserId();

        ArgumentException.ThrowIfNullOrEmpty(userId, nameof(user));

        UpdatedBy = string.IsNullOrWhiteSpace(e.UpdatedBy) ? userId : e.UpdatedBy;

        UpdatedDate = e.UpdatedDate;
    }

    /// <summary>
    /// Method for generating an updated entity based on view model values. This method is where you update the <typeparamref name="UpdateDate"/>
    /// </summary>
    /// <returns></returns>
    public abstract T GenerateUpdatedEntity();
}
