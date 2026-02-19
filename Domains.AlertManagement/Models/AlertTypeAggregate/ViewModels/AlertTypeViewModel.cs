namespace Domains.AlertManagement.AlertTypeAggregate.ViewModels;

public sealed class AlertTypeViewModel : ViewModelBase
{
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

    private DateTime _createdAt;

    [Display(Name = "Created At")]
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    private DateTime _lastModifiedAt;

    [Display(Name = "Last Modified At")]
    public DateTime LastModifiedAt
    {
        get => _lastModifiedAt;
        set => SetProperty(ref _lastModifiedAt, value);
    }

    public static AlertTypeViewModel FromResponse(AlertTypeResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description,
        Color = response.Color,
        NotifyUserIds = response.NotifyUserIds,
        CreatedAt = response.CreatedAt,
        LastModifiedAt = response.LastModifiedAt
    };
}
