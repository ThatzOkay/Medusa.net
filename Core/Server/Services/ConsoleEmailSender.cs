using Abstractions.Entities;
using Microsoft.AspNetCore.Identity;
using Server.Utils;

namespace Server.Services;

/// <summary>
/// Stand-in <see cref="IEmailSender{TUser}"/> that logs the link to the console instead of
/// actually sending an email. Wire up a real sender later; until then this keeps the Identity
/// API's register/forgotPassword flows usable during development.
/// </summary>
public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    : IEmailSender<User>
{
    private readonly ILogger<ConsoleEmailSender> _logger = logger;

    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        _logger.LogInformation("Confirmation link for {Email}: {Link}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        _logger.LogInformation("Password reset link for {Email}: {Link}", email, resetLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        var link = BuildResetPasswordLink(email, resetCode);
        _logger.LogInformation("Password reset link for {Email}: {Link}", email, link);
        return Task.CompletedTask;
    }
    
    private static string BuildResetPasswordLink(string email, string resetCode) =>
        $"{ServerAddress.PublicUrl.TrimEnd('/')}/auth/resetPassword?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(resetCode)}";
}
