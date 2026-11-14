namespace Domains.AlertManagement.AlertStatusLevelAggregate.ViewModels;

using System.Security.Claims;

/// <summary>
/// View model for creating or editing an alert status level.
/// </summary>
public sealed class AlertStatusLevelViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a view model for a new alert status level.
    /// </summary>
    public AlertStatusLevelViewModel(ClaimsPrincipal user) : base(user)
    {
    }

    /// <summary>
    /// Initializes a view model for an existing alert status level.
    /// </summary>
    public AlertStatusLevelViewModel(string id, ClaimsPrincipal user) : base(id, user)
    { }

    /// <summary>
    /// Backing field for <see cref="Name"/>.
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Display(Name = "Name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Backing field for <see cref="Description"/>.
    /// </summary>
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Display(Name = "Description")]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Backing field for <see cref="BackgroundColor"/>.
    /// </summary>
    private string _backgroundColor = string.Empty;

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    [Display(Name = "Background Color")]
    public string BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    /// <summary>
    /// Backing field for <see cref="TextColor"/>.
    /// </summary>
    private string _textColor = string.Empty;

    /// <summary>
    /// Gets or sets the text color.
    /// </summary>
    [Display(Name = "Text Color")]
    public string TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    /// <summary>
    /// Backing field for <see cref="SendNotifications"/>.
    /// </summary>
    private bool _sendNotifications;

    /// <summary>
    /// Gets or sets a value indicating whether notifications are sent when an alert reaches this level.
    /// </summary>
    [Display(Name = "Send Notifications")]
    public bool SendNotifications
    {
        get => _sendNotifications;
        set => SetProperty(ref _sendNotifications, value);
    }

    /// <summary>
    /// Backing field for <see cref="IsDefault"/>.
    /// </summary>
    private bool _isDefault;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default level for new alerts.
    /// </summary>
    [Display(Name = "Is Default")]
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    /// <summary>
    /// Backing field for <see cref="IsCanceled"/>.
    /// </summary>
    private bool _isCanceled;

    /// <summary>
    /// Gets or sets a value indicating whether the level is canceled.
    /// </summary>
    [Display(Name = "Is Canceled")]
    public bool IsCanceled
    {
        get => _isCanceled;
        set => SetProperty(ref _isCanceled, value);
    }

    /// <summary>
    /// Creates a view model from an API response.
    /// </summary>
    public static AlertStatusLevelViewModel FromResponse(AlertStatusLevelResponse response) => new(response.Id, new ClaimsPrincipal())
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description,
        BackgroundColor = response.BackgroundColor,
        TextColor = response.TextColor,
        SendNotifications = response.SendNotifications,
        IsDefault = response.IsDefault,
        IsCanceled = response.IsCanceled,
        CreatedBy = response.CreatedBy,
        UpdatedBy = response.UpdatedBy,
        CreatedDate = response.CreatedDate,
        UpdatedDate = response.UpdatedDate
    };
}
