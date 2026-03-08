using AlertOne.Api.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.Core.Common;
using UserManagement.Core.DTOs.Requests;
using UserManagement.Core.DTOs.Responses;
using UserManagement.Core.Interfaces;
using UserManagement.Core.ValueObjects;
using UserManagement.Infrastructure.Identity;

namespace UserManagement.Infrastructure.Services;

/// <summary>
/// Service for user invitation operations
/// </summary>
internal sealed class InvitationService(
    ILogger<InvitationService> logger,
    ApplicationDbContext dbContext,
    IEmailService emailService,
    UserManager<ApplicationUser> userManager,
    IUserSettingsService userSettingsService) : IInvitationService
{
    public async Task<Result<InvitationResponse>> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken = default)
    {
        logger
            .LogInformation("Creating invitation for email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result.Failure<InvitationResponse>(emailResult.Error!);
        }

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(emailResult.Value.Value);

        if (existingUser is not null)
        {
            logger
                .LogWarning("Invitation creation failed: User with email {Email} already exists", request.Email);

            return Result
                .Failure<InvitationResponse>(Error.Conflict("Invitation.UserExists", $"A user with email {request.Email} already exists"));
        }

        // Check if there's already a pending invitation for this email
        var existingInvitation = await dbContext
            .UserInvitations
            .FirstOrDefaultAsync(i => i.Email == emailResult.Value.Value && !i.IsAccepted, cancellationToken);

        if (existingInvitation is not null)
        {
            logger.LogWarning("Invitation creation failed: Pending invitation already exists for {Email}", request.Email);

            return Result
                .Failure<InvitationResponse>(Error.Conflict("Invitation.AlreadyExists", $"A pending invitation already exists for {request.Email}"));
        }

        var invitation = new UserInvitationEntity
        {
            Id = Guid.NewGuid().ToString(),
            Email = emailResult.Value.Value,
            ActivationCode = GenerateActivationCode(),
            InvitedBy = request.InvitedBy,
            InvitedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsAccepted = false
        };

        dbContext.UserInvitations.Add(invitation);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Send invitation email
        try
        {
            await emailService.SendInvitationEmailAsync(invitation.Email, invitation.ActivationCode, invitation.InvitedBy, cancellationToken);

            logger
                .LogInformation("Invitation email sent successfully to {Email}", invitation.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send invitation email to {Email}", invitation.Email);
            // Continue - the invitation is created even if email fails
        }

        logger.LogInformation("Invitation created successfully: {InvitationId}", invitation.Id);

        return Result.Success(MapToResponse(invitation));
    }

    public async Task<Result<IEnumerable<InvitationResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all invitations");

        var invitations = await dbContext
            .UserInvitations
            .OrderByDescending(i => i.InvitedAt)
            .ToListAsync(cancellationToken);

        var responses = invitations.Select(MapToResponse);

        return Result.Success(responses);
    }

    public async Task<Result<InvitationResponse>> GetByCodeAsync(string activationCode, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting invitation by activation code");

        if (string.IsNullOrWhiteSpace(activationCode))
        {
            return Result.Failure<InvitationResponse>(
                Error.Validation("Invitation.CodeEmpty", "Activation code cannot be empty"));
        }

        var invitation = await dbContext
            .UserInvitations
            .FirstOrDefaultAsync(i => i.ActivationCode == activationCode, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invitation not found with activation code");

            return Result.Failure<InvitationResponse>(
                Error.NotFound("Invitation.NotFound", "Invitation not found"));
        }

        return Result.Success(MapToResponse(invitation));
    }

    public async Task<Result<InvitationResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting invitation by ID: {InvitationId}", id);

        var invitation = await dbContext
            .UserInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invitation not found: {InvitationId}", id);

            return Result.Failure<InvitationResponse>(
                Error.NotFound("Invitation.NotFound", $"Invitation with ID {id} not found"));
        }

        return Result.Success(MapToResponse(invitation));
    }

    public async Task<Result<InvitationResponse>> ValidateCodeAsync(string activationCode, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validating activation code");

        if (string.IsNullOrWhiteSpace(activationCode))
        {
            return Result.Failure<InvitationResponse>(
                Error.Validation("Invitation.CodeEmpty", "Activation code cannot be empty"));
        }

        var invitation = await dbContext
            .UserInvitations
            .FirstOrDefaultAsync(i => i.ActivationCode == activationCode, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invalid activation code");

            return Result.Failure<InvitationResponse>(
                Error.NotFound("Invitation.InvalidCode", "Invalid activation code"));
        }

        if (invitation.IsAccepted)
        {
            return Result.Failure<InvitationResponse>(
                Error.Conflict("Invitation.AlreadyAccepted", "This invitation has already been accepted"));
        }

        if (DateTime.UtcNow > invitation.ExpiresAt)
        {
            return Result.Failure<InvitationResponse>(
                Error.Validation("Invitation.Expired", "This invitation has expired"));
        }

        return Result.Success(MapToResponse(invitation));
    }

    public async Task<Result<UserResponse>> AcceptAsync(AcceptInvitationRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Accepting invitation with activation code");

        if (string.IsNullOrWhiteSpace(request.ActivationCode))
        {
            return Result.Failure<UserResponse>(
                Error.Validation("Invitation.CodeEmpty", "Activation code cannot be empty"));
        }

        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.ActivationCode == request.ActivationCode, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invitation not found with activation code");

            return Result.Failure<UserResponse>(
                Error.NotFound("Invitation.InvalidCode", "Invalid activation code"));
        }

        if (invitation.IsAccepted)
        {
            return Result.Failure<UserResponse>(
                Error.Conflict("Invitation.AlreadyAccepted", "This invitation has already been accepted"));
        }

        if (DateTime.UtcNow > invitation.ExpiresAt)
        {
            return Result.Failure<UserResponse>(
                Error.Validation("Invitation.Expired", "This invitation has expired"));
        }

        // Validate first name
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result.Failure<UserResponse>(
                Error.Validation("User.FirstNameEmpty", "First name is required"));
        }

        if (request.FirstName.Length > 100)
        {
            return Result.Failure<UserResponse>(
                Error.Validation("User.FirstNameTooLong", "First name must not exceed 100 characters"));
        }

        // Validate last name
        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure<UserResponse>(
                Error.Validation("User.LastNameEmpty", "Last name is required"));
        }

        if (request.LastName.Length > 100)
        {
            return Result.Failure<UserResponse>(
                Error.Validation("User.LastNameTooLong", "Last name must not exceed 100 characters"));
        }

        // Create the user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = invitation.Email,
            Email = invitation.Email,
            NormalizedUserName = invitation.Email.ToUpperInvariant(),
            NormalizedEmail = invitation.Email.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            logger.LogWarning("User creation failed: {Errors}", errors);

            return Result.Failure<UserResponse>(
                Error.Validation("User.CreationFailed", errors));
        }

        // Assign default role
        var roleResult = await userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to assign role to user {UserId}: {Errors}", user.Id, errors);
        }

        // Create default user settings
        var userId = UserId.From(user.Id);
        await userSettingsService.CreateDefaultAsync(userId, cancellationToken);

        // Mark invitation as accepted
        invitation.IsAccepted = true;
        invitation.AcceptedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invitation accepted successfully. User created: {UserId}", user.Id);

        var roles = await userManager.GetRolesAsync(user);

        return Result.Success(new UserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles,
            user.CreatedAt,
            user.LastModifiedAt
        ));
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting invitation: {InvitationId}", id);

        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invitation not found: {InvitationId}", id);

            return Result.Failure(
                Error.NotFound("Invitation.NotFound", $"Invitation with ID {id} not found"));
        }

        dbContext.UserInvitations.Remove(invitation);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invitation deleted successfully: {InvitationId}", id);

        return Result.Success();
    }

    public async Task<Result<InvitationResponse>> ResendAsync(string id, string resendBy, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Resending invitation: {InvitationId}", id);

        if (string.IsNullOrWhiteSpace(resendBy))
        {
            return Result.Failure<InvitationResponse>(
                Error.Validation("Invitation.ResendByEmpty", "Resender name cannot be empty"));
        }

        var invitation = await dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invitation is null)
        {
            logger.LogWarning("Invitation not found: {InvitationId}", id);

            return Result.Failure<InvitationResponse>(
                Error.NotFound("Invitation.NotFound", $"Invitation with ID {id} not found"));
        }

        if (invitation.IsAccepted)
        {
            return Result.Failure<InvitationResponse>(
                Error.Conflict("Invitation.AlreadyAccepted", "Cannot resend an already accepted invitation"));
        }

        // Generate new activation code and extend expiration
        invitation.ActivationCode = GenerateActivationCode();
        invitation.InvitedBy = resendBy;
        invitation.InvitedAt = DateTime.UtcNow;
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Send invitation email
        try
        {
            await emailService.SendInvitationEmailAsync(invitation.Email, invitation.ActivationCode, invitation.InvitedBy, cancellationToken);

            logger.LogInformation("Invitation email resent successfully to {Email}", invitation.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend invitation email to {Email}", invitation.Email);
            // Continue - the invitation is updated even if email fails
        }

        logger.LogInformation("Invitation resent successfully: {InvitationId}", id);

        return Result.Success(MapToResponse(invitation));
    }

    private static string GenerateActivationCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

    private static InvitationResponse MapToResponse(UserInvitationEntity entity)
    {
        var isExpired = DateTime.UtcNow > entity.ExpiresAt;
        var status = entity.IsAccepted ? "Accepted" : (isExpired ? "Expired" : "Pending");

        return new InvitationResponse(
            entity.Id,
            entity.Email,
            entity.InvitedBy,
            entity.InvitedAt,
            entity.ExpiresAt,
            entity.AcceptedAt,
            entity.IsAccepted,
            isExpired,
            status
        );
    }
}
