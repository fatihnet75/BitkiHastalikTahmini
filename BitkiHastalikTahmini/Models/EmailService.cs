using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BitkiHastalikTahmini.Models
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmail(string toEmail, string verificationCode)
        {
            try
            {
                string body = $"Bitki Hastalık Tahmini uygulaması için doğrulama kodunuz: <b>{verificationCode}</b><br><br>" +
                              "Bu kod 10 dakika süreyle geçerlidir.<br><br>" +
                              "Eğer bu işlemi siz yapmadıysanız, bu e-postayı dikkate almayınız.";

                // Email yapılandırmasını oku
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPortStr = _configuration["Email:SmtpPort"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];

                Console.WriteLine("E-posta Gönderme Yapılandırması:");
                Console.WriteLine($"SMTP: {smtpServer}");
                Console.WriteLine($"Port: {smtpPortStr}");
                Console.WriteLine($"Kullanıcı: {username}");
                Console.WriteLine($"Şifre var mı: {!string.IsNullOrEmpty(password)}");

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) || 
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("E-posta ayarları eksik! SMTP ayarlarını kontrol edin.");
                }

                int smtpPort = int.Parse(smtpPortStr);

                var fromAddress = new MailAddress(fromEmail, fromName);
                var toAddress = new MailAddress(toEmail);
                const string subject = "Email Doğrulama Kodu";

                // Konsolda bilgileri yazdır (debug için)
                Console.WriteLine($"SMTP Bağlantısı yapılıyor: {smtpServer}:{smtpPort}");
                Console.WriteLine($"Kullanıcı: {username}");
                Console.WriteLine($"Gönderici: {fromEmail}");
                Console.WriteLine($"Alıcı: {toEmail}");
                Console.WriteLine($"Doğrulama Kodu: {verificationCode}");

                using (var smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    Timeout = 30000 // 30 saniye timeout
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    // Gmail'de "Daha az güvenli uygulamalara izin ver" seçeneği gerekiyor
                    // veya uygulama şifresi kullanılmalı
                    Console.WriteLine("Mail gönderiliyor...");
                    await smtp.SendMailAsync(message);
                    Console.WriteLine("Mail gönderildi!");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Email gönderme hatası: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç Hata: {ex.InnerException.Message}");
                }
                
                // Hatayı fırlat, böylece uygulama bilgilendirilebilir
                throw;
            }
        }
        
        public async Task SendPasswordResetEmail(string toEmail, string verificationCode)
        {
            try
            {
                string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            border: 1px solid #eee;
                            border-radius: 10px;
                        }}
                        .header {{
                            text-align: center;
                            padding-bottom: 20px;
                            border-bottom: 1px solid #eee;
                        }}
                        .header h2 {{
                            color: #4CAF50;
                        }}
                        .content {{
                            padding: 20px 0;
                        }}
                        .code {{
                            font-size: 24px;
                            font-weight: bold;
                            color: #4CAF50;
                            padding: 10px;
                            text-align: center;
                            margin: 20px 0;
                            letter-spacing: 5px;
                        }}
                        .footer {{
                            text-align: center;
                            font-size: 12px;
                            color: #777;
                            padding-top: 20px;
                            border-top: 1px solid #eee;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Dr. PlantAI - Şifre Sıfırlama</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba,</p>
                            <p>Hesabınız için bir şifre sıfırlama talebinde bulundunuz. Şifrenizi sıfırlamak için aşağıdaki doğrulama kodunu kullanın:</p>
                            <div class='code'>{verificationCode}</div>
                            <p>Bu kod 10 dakika süreyle geçerlidir.</p>
                            <p>Eğer bu işlemi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>
                        </div>
                        <div class='footer'>
                            <p>Bu e-posta, Dr.PlantAI Bitki Hastalık Tahmini uygulaması tarafından otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

                // Email yapılandırmasını oku
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPortStr = _configuration["Email:SmtpPort"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) || 
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("E-posta ayarları eksik! SMTP ayarlarını kontrol edin.");
                }

                int smtpPort = int.Parse(smtpPortStr);

                var fromAddress = new MailAddress(fromEmail, fromName);
                var toAddress = new MailAddress(toEmail);
                const string subject = "Şifre Sıfırlama - Dr.PlantAI";

                using (var smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    Timeout = 30000 // 30 saniye timeout
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    Console.WriteLine($"Şifre sıfırlama e-postası gönderiliyor... Alıcı: {toEmail}");
                    await smtp.SendMailAsync(message);
                    Console.WriteLine("Şifre sıfırlama e-postası gönderildi!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şifre sıfırlama e-posta gönderme hatası: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç Hata: {ex.InnerException.Message}");
                }
                
                throw;
            }
        }
    }
} 