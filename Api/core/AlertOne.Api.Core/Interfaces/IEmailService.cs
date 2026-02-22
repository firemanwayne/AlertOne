namespace AlertOne.Api.Core.Interfaces;

/// <summary>
/// Interface for sending emails via Azure Communication Services.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email with the specified subject and HTML body.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlBody">The HTML body content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully.</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an invitation email with an activation code.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="activationCode">The 6-digit activation code.</param>
    /// <param name="invitedBy">The name of the person who sent the invitation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully.</returns>
    Task<bool> SendInvitationEmailAsync(string to, string activationCode, string invitedBy, CancellationToken cancellationToken = default);
}
