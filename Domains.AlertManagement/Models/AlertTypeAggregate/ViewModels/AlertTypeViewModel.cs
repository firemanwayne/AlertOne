namespace Domains.AlertManagement.AlertTypeAggregate.ViewModels;

using System.Security.Claims;

public sealed class AlertTypeViewModel : ViewModelBase
{
    public AlertTypeViewModel(ClaimsPrincipal user) : base(user)
    {
    }

    public AlertTypeViewModel(string id, ClaimsPrincipal user) : base(id, user)
    {
    }

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

    private string _color = string.Empty;

    [Display(Name = "Color")]
    public string Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }

    private IReadOnlyList<string> _notifyUserIds = [];

    [Display(Name = "Notify Users")]
    public IReadOnlyList<string> NotifyUserIds
    {
        get => _notifyUserIds;
        set => SetProperty(ref _notifyUserIds, value);
    }

    public static AlertTypeViewModel FromResponse(AlertTypeResponse response) => new(response.Id, new ClaimsPrincipal())
    {
        Name = response.Name,
        Description = response.Description,
        Color = response.Color,
        NotifyUserIds = response.NotifyUserIds,
        CreatedDate = response.CreatedDate,
        UpdatedDate = response.UpdatedDate,
        CreatedBy = response.CreatedBy,
        UpdatedBy = response.UpdatedBy,
    };
}
