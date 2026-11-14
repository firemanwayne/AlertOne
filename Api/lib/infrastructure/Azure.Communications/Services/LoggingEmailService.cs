using AlertOne.Api.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Azure.Communications.Services;

/// <summary>
/// Local development stand-in for <see cref="IEmailService"/> that only logs. Used when no Communication Services connection string is configured.
/// </summary>
internal sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    /// <inheritdoc />
    public Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Email to {To} skipped: {Subject}", to, subject);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> SendInvitationEmailAsync(string to, string activationCode, string invitedBy, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Local] Invitation email to {To} from {InvitedBy} skipped. Activation code: {ActivationCode}", to, invitedBy, activationCode);

        return Task.FromResult(true);
    }
}
