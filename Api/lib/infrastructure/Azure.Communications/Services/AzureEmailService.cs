using AlertOne.Api.Core.Interfaces;
using Azure.Communication.Email;
using Azure.Communications.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.Communications.Services;

/// <summary>
/// Implementation of email service using Azure Communication Services.
/// </summary>
internal sealed class AzureEmailService(
    IOptions<AzureCommunicationsOptions> options,
    ILogger<AzureEmailService> logger) : IEmailService
{
    /// <summary>
    /// The bound Azure Communication Services options.
    /// </summary>
    private readonly AzureCommunicationsOptions _options = options.Value;
    /// <summary>
    /// The Azure Communication Services email client.
    /// </summary>
    private readonly EmailClient _client = new(options.Value.ConnectionString);

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending email to {Recipient} with subject: {Subject}", to, subject);

        try
        {
            var emailContent = new EmailContent(subject)
            {
                Html = htmlBody
            };

            var emailMessage = new EmailMessage(
                senderAddress: _options.SenderAddress,
                recipientAddress: to,
                content: emailContent);

            var operation = await _client.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);

            if (operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded)
            {
                logger.LogInformation("Email sent successfully to {Recipient}", to);
                return true;
            }

            logger.LogWarning("Email send operation did not succeed. Status: {Status}", operation.Value.Status);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipient}", to);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendInvitationEmailAsync(string to, string activationCode, string invitedBy, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending invitation email to {Recipient} from {InvitedBy}", to, invitedBy);

        var webAppUrl = _options.WebAppBaseUrl.TrimEnd('/');
        var mobileScheme = _options.MobileAppScheme;

        var htmlBody = BuildInvitationEmailHtml(activationCode, invitedBy, webAppUrl, mobileScheme);
        var subject = "You've been invited to AlertOne";

        return await SendEmailAsync(to, subject, htmlBody, cancellationToken);
    }

    /// <summary>
    /// Builds the HTML body of the invitation email, including web and mobile deep links.
    /// </summary>
    private static string BuildInvitationEmailHtml(string activationCode, string invitedBy, string webAppUrl, string mobileScheme)
    {
        var acceptUrl = $"{webAppUrl}/accept-invitation?code={activationCode}";
        var mobileDeepLink = $"{mobileScheme}://accept-invitation?code={activationCode}";

        return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>AlertOne Invitation</title>
            </head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;">
                    <h1 style="color: white; margin: 0; font-size: 28px;">AlertOne</h1>
                </div>

                <div style="background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;">
                    <h2 style="color: #333; margin-top: 0;">You've Been Invited!</h2>

                    <p><strong>{invitedBy}</strong> has invited you to join AlertOne.</p>

                    <p>Use the activation code below to complete your registration:</p>

                    <div style="background: #f5f5f5; border: 2px dashed #667eea; border-radius: 8px; padding: 20px; text-align: center; margin: 25px 0;">
                        <span style="font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #667eea;">{activationCode}</span>
                    </div>

                    <p style="text-align: center;">Or click the button below to accept your invitation:</p>

                    <div style="text-align: center; margin: 25px 0;">
                        <a href="{acceptUrl}" style="display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; padding: 14px 30px; border-radius: 6px; font-weight: bold; font-size: 16px;">Accept Invitation</a>
                    </div>

                    <p style="font-size: 14px; color: #666;">
                        <strong>On mobile?</strong> <a href="{mobileDeepLink}" style="color: #667eea;">Open in AlertOne app</a>
                    </p>

                    <hr style="border: none; border-top: 1px solid #e0e0e0; margin: 25px 0;">

                    <p style="font-size: 12px; color: #999; margin-bottom: 0;">
                        This invitation will expire in 7 days. If you didn't expect this invitation, you can safely ignore this email.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
