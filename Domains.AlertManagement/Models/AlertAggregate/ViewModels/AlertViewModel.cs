using Domains.AlertManagement.AlertAggregate.Dtos;
using Domains.AlertManagement.AlertStatusLevelAggregate.ViewModels;
using Domains.AlertManagement.AlertTypeAggregate.ViewModels;
using UserManagementApps.Core.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Domains.AlertManagement.AlertAggregate.ViewModels;

public sealed class AlertViewModel : ViewModelBase
{
    private AlertTypeViewModel _type = new();

    [Display(Name = "Alert Type")]
    public AlertTypeViewModel Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private AlertStatusLevelViewModel _statusLevel = new();

    [Display(Name = "Status Level")]
    public AlertStatusLevelViewModel StatusLevel
    {
        get => _statusLevel;
        set => SetProperty(ref _statusLevel, value);
    }

    private string _createdBy = string.Empty;

    [Display(Name = "Created By")]
    public string CreatedBy
    {
        get => _createdBy;
        set => SetProperty(ref _createdBy, value);
    }

    private string? _lastModifiedBy;

    [Display(Name = "Last Modified By")]
    public string? LastModifiedBy
    {
        get => _lastModifiedBy;
        set => SetProperty(ref _lastModifiedBy, value);
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

    public static AlertViewModel FromResponse(AlertResponse response) => new()
    {
        Id = response.Id,
        Type = AlertTypeViewModel.FromResponse(response.Type),
        StatusLevel = AlertStatusLevelViewModel.FromResponse(response.StatusLevel),
        CreatedBy = response.CreatedBy,
        LastModifiedBy = response.LastModifiedBy,
        CreatedAt = response.CreatedAt,
        LastModifiedAt = response.LastModifiedAt,
        IsActive = response.IsActive,
        RescindedAt = response.RescindedAt,
        RescindedBy = response.RescindedBy
    };
}
