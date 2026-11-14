namespace Domains.AlertManagement.AlertTypeAggregate.ViewModels;

using System.Security.Claims;

/// <summary>
/// View model for creating or editing an alert type.
/// </summary>
public sealed class AlertTypeViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a view model for a new alert type.
    /// </summary>
    public AlertTypeViewModel(ClaimsPrincipal user) : base(user)
    {
    }

    /// <summary>
    /// Initializes a view model for an existing alert type.
    /// </summary>
    public AlertTypeViewModel(string id, ClaimsPrincipal user) : base(id, user)
    {
    }

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
    /// Backing field for <see cref="NotifyUserIds"/>.
    /// </summary>
    private IReadOnlyList<string> _notifyUserIds = [];

    /// <summary>
    /// Gets or sets the identifiers of the users notified for this alert type.
    /// </summary>
    [Display(Name = "Notify Users")]
    public IReadOnlyList<string> NotifyUserIds
    {
        get => _notifyUserIds;
        set => SetProperty(ref _notifyUserIds, value);
    }

    /// <summary>
    /// Creates a view model from an API response.
    /// </summary>
    public static AlertTypeViewModel FromResponse(AlertTypeResponse response) => new(response.Id, new ClaimsPrincipal())
    {
        Name = response.Name,
        Description = response.Description,
        BackgroundColor = response.BackgroundColor,
        TextColor = response.TextColor,
        NotifyUserIds = response.NotifyUserIds,
        CreatedDate = response.CreatedDate,
        UpdatedDate = response.UpdatedDate,
        CreatedBy = response.CreatedBy,
        UpdatedBy = response.UpdatedBy,
    };
}
