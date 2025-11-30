using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KutuphaneOtomasyonu.Services
{
    /// <summary>
    /// Email bildirimleri için servis. SMTP üzerinden email gönderir.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly bool _isEnabled;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _isEnabled = _configuration.GetValue<bool>("Email:Enabled", false);
        }

        /// <summary>
        /// Email gönderir.
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            if (!_isEnabled)
            {
                _logger.LogInformation("Email servisi devre dışı. Simülasyon: {To} - {Subject}", to, subject);
                Console.WriteLine($"📧 [Email Simülasyonu] To: {to}, Subject: {subject}");
                return true; // Simülasyon modunda başarılı döner
            }

            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
                var fromName = _configuration["Email:FromName"] ?? "Kütüphane Otomasyonu";

                if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
                {
                    _logger.LogWarning("SMTP bilgileri yapılandırılmamış. Email gönderilemedi.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email başarıyla gönderildi: {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gönderme hatası: {To}", to);
                return false;
            }
        }

        /// <summary>
        /// Gecikme bildirimi gönderir.
        /// </summary>
        public async Task<bool> SendOverdueNotificationAsync(string memberEmail, string memberName, string bookTitle, int daysLate, decimal penaltyAmount)
        {
            var subject = "Gecikmiş Kitap Bildirimi";
            var body = $@"
<h2>Sayın {memberName},</h2>
<p>'{bookTitle}' kitabınız <strong>{daysLate} gün</strong> gecikmiştir.</p>
<p>Toplam gecikme cezası: <strong>{penaltyAmount:C}</strong></p>
<p>Lütfen en kısa sürede kitabı iade edin veya ödemeyi yapın.</p>
<p>İyi günler dileriz.</p>
<p><em>Kütüphane Yönetimi</em></p>
";

            return await SendEmailAsync(memberEmail, subject, body);
        }

        /// <summary>
        /// Rezervasyon bildirimi gönderir.
        /// </summary>
        public async Task<bool> SendReservationNotificationAsync(string memberEmail, string memberName, string bookTitle)
        {
            var subject = "Rezervasyon Bildirimi - Kitap Müsait";
            var body = $@"
<h2>Sayın {memberName},</h2>
<p>Rezerve ettiğiniz '{bookTitle}' kitabı artık müsait durumda!</p>
<p>Lütfen en kısa sürede kütüphaneye gelerek kitabı ödünç alın.</p>
<p>İyi günler dileriz.</p>
<p><em>Kütüphane Yönetimi</em></p>
";

            return await SendEmailAsync(memberEmail, subject, body);
        }
    }
}




