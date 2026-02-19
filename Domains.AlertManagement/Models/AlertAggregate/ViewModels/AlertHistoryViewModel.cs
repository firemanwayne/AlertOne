namespace Domains.AlertManagement.AlertAggregate.ViewModels;

public sealed class AlertHistoryViewModel : ViewModelBase
{
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

    private string _changedBy = string.Empty;

    [Display(Name = "Changed By")]
    public string ChangedBy
    {
        get => _changedBy;
        set => SetProperty(ref _changedBy, value);
    }

    private DateTime _changedAt;

    [Display(Name = "Changed At")]
    public DateTime ChangedAt
    {
        get => _changedAt;
        set => SetProperty(ref _changedAt, value);
    }

    private string? _notes;

    [Display(Name = "Notes")]
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public static AlertHistoryViewModel FromResponse(AlertHistoryResponse response) => new()
    {
        Id = response.Id,
        AlertId = response.AlertId,
        EventType = response.EventType,
        ChangedBy = response.ChangedBy,
        ChangedAt = response.ChangedAt,
        Notes = response.Notes
    };
}
