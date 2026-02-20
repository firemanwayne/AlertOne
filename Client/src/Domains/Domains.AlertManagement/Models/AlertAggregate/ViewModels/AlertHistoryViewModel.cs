#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Domains.AlertManagement.AlertAggregate.ViewModels;

using System.Security.Claims;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class AlertHistoryViewModel : ViewModelBase
{
    public AlertHistoryViewModel(ClaimsPrincipal user) : base(user)
    {
    }

    public AlertHistoryViewModel(string id, ClaimsPrincipal user) : base(id, user)
    {
    }

    private string _alertId = string.Empty;

    [Display(Name = "Alert ID")]
    public string AlertId
    {
        get => _alertId;
        set => SetProperty(ref _alertId, value);
    }

    private string _eventType = string.Empty;

    [Display(Name = "Event")]
    public string EventType
    {
        get => _eventType;
        set => SetProperty(ref _eventType, value);
    }

    private string? _notes;



    [Display(Name = "Notes")]
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public static AlertHistoryViewModel FromResponse(AlertHistoryResponse response) => new(response.Id, new ClaimsPrincipal())
    {
        Id = response.Id,
        AlertId = response.AlertId,
        EventType = response.EventType,
        CreatedBy = response.CreatedBy,
        UpdatedBy = response.UpdatedBy,
        CreatedDate = response.CreatedDate,
        UpdatedDate = response.UpdatedDate,
        Notes = response.Notes
    };
}
