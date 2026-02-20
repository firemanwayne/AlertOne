#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertAggregate.ViewModels;

using System.Security.Claims;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class AlertViewModel : ViewModelBase
{
    public AlertViewModel(ClaimsPrincipal user) : base(user)
    {
        IsNew = true;
        _type = new(user);
        _statusLevel = new(user);
    }

    public AlertViewModel(string id, ClaimsPrincipal user) : base(id, user)
    {
        IsNew = false;
        _type = new(user);
        _statusLevel = new(user);
    }

    private AlertTypeViewModel _type;

    [Display(Name = "Alert Type")]
    public AlertTypeViewModel Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private AlertStatusLevelViewModel _statusLevel;

    [Display(Name = "Status Level")]
    public AlertStatusLevelViewModel StatusLevel
    {
        get => _statusLevel;
        set => SetProperty(ref _statusLevel, value);
    }

    private bool _isActive;

    [Display(Name = "Active")]
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    private DateTime? _rescindedAt;

    [Display(Name = "Rescinded At")]
    public DateTime? RescindedAt
    {
        get => _rescindedAt;
        set => SetProperty(ref _rescindedAt, value);
    }

    private string? _rescindedBy;

    [Display(Name = "Rescinded By")]
    public string? RescindedBy
    {
        get => _rescindedBy;
        set => SetProperty(ref _rescindedBy, value);
    }

    public static AlertViewModel FromResponse(AlertResponse response) => new(response.Id, new ClaimsPrincipal())
    {
        Type = AlertTypeViewModel.FromResponse(response.Type),
        StatusLevel = AlertStatusLevelViewModel.FromResponse(response.StatusLevel),
        CreatedBy = response.CreatedBy,
        UpdatedDate = response.UpdatedDate,
        CreatedDate = response.CreatedDate,
        UpdatedBy = response.UpdatedBy,
        IsActive = response.IsActive,
        RescindedAt = response.RescindedAt,
        RescindedBy = response.RescindedBy
    };
}
