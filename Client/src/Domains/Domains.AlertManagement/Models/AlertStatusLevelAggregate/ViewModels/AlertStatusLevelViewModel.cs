namespace Domains.AlertManagement.AlertStatusLevelAggregate.ViewModels;

using System.Security.Claims;

public sealed class AlertStatusLevelViewModel : ViewModelBase
{
    public AlertStatusLevelViewModel(ClaimsPrincipal user) : base(user)
    {
    }

    public AlertStatusLevelViewModel(string id, ClaimsPrincipal user) : base(id, user)
    { }

    private string _name = string.Empty;

    [Display(Name = "Name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = string.Empty;

    [Display(Name = "Description")]
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _backgroundColor = string.Empty;

    [Display(Name = "Background Color")]
    public string BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    private string _textColor = string.Empty;

    [Display(Name = "Text Color")]
    public string TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    private bool _sendNotifications;

    [Display(Name = "Send Notifications")]
    public bool SendNotifications
    {
        get => _sendNotifications;
        set => SetProperty(ref _sendNotifications, value);
    }

    private bool _isDefault;

    [Display(Name = "Is Default")]
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    private bool _isCanceled;

    [Display(Name = "Is Canceled")]
    public bool IsCanceled
    {
        get => _isCanceled;
        set => SetProperty(ref _isCanceled, value);
    }

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
