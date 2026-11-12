namespace AuthorizationModuleAPI.Services;

using AuthorizationModuleAPI.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

public interface IEmailService
{
    void Send(string to, string subject, string html, string from = null);
}

public class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<AppSettings> appSettings, ILogger<EmailService> logger)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public void Send(string to, string subject, string html, string from = null)
    {
        // Формуємо лист для підтвердження реєстрації або скидання пароля
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(from ?? _appSettings.EmailFrom));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        try
        {
            // Намагаємося відправити через налаштований SMTP-сервер (наприклад, Ethereal у appsettings.json)
            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
        catch (Exception ex)
        {
            // У навчальному/локальному стенді (якщо облікові дані Ethereal прострочені або немає мережі)
            // не блокуємо реєстрацію користувача, а логуємо вміст листа у консоль Aspire.
            _logger.LogWarning(
                ex,
                "SMTP-відправка не вдалася (локальний fallback). Лист для {To} | Тема: {Subject} | Текст: {Body}",
                to,
                subject,
                html);
        }
    }
}