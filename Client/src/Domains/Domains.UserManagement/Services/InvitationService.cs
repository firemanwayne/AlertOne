namespace Domains.UserManagement.Services;

/// <summary>
/// Service for user invitation operations
/// </summary>
internal sealed class InvitationService(
    IUserManagementApiClient apiClient) : IInvitationService
{
    public async Task<IApiResult<IEnumerable<InvitationResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await apiClient
            .GetAsync<IEnumerable<InvitationResponse>>(
            "/api/invitations",
            cancellationToken);
    }

    public async Task<IApiResult<InvitationResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .GetAsync<InvitationResponse>(
            $"/api/invitations/{id}",
            cancellationToken);
    }

    public async Task<IApiResult<InvitationResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .PostAsync<CreateInvitationRequest, InvitationResponse>(
            "/api/invitations",
            request,
            cancellationToken);
    }

    public async Task<IApiResult<InvitationResponse>> ValidateCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .GetAsync<InvitationResponse>(
            $"/api/invitations/validate/{code}",
            cancellationToken);
    }

    public async Task<IApiResult<UserResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .PostAsync<AcceptInvitationRequest, UserResponse>(
            "/api/invitations/accept",
            request,
            cancellationToken);
    }

    public async Task<IApiResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .DeleteAsync(
            $"/api/invitations/{id}",
            cancellationToken);
    }

    public async Task<IApiResult<InvitationResponse>> ResendAsync(string id, ResendInvitationRequest request, CancellationToken cancellationToken = default)
    {
        return await apiClient
            .PostAsync<ResendInvitationRequest, InvitationResponse>(
            $"/api/invitations/{id}/resend",
            request,
            cancellationToken);
    }
}
