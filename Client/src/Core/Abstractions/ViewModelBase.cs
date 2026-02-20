namespace AlertOne.Core.Abstractions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Serialization;

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

    public bool SpinnerVisible { get; set; }

    public bool DisableSpinnerButton { get; set; }

    public string AddEditButtonText => IsNew ? "Submit" : "Update";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public void ShowSpinnerAndDisableButton()
    {
        SpinnerVisible = true;
        DisableSpinnerButton = true;

        OnPropertyChanged();
    }

    public void HideSpinnerAndEnableButton()
    {
        SpinnerVisible = false;
        DisableSpinnerButton = false;

        OnPropertyChanged();
    }

    /// <summary>
    /// Convenience property to set the url segment of index page
    /// </summary>
    public static string IndexSegment { get; set; } = string.Empty;

    [JsonIgnore] public ClaimsPrincipal CurrentUser { get; set; }

    public override int GetHashCode() => HashCode.Combine(Id, UpdatedDate);

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
