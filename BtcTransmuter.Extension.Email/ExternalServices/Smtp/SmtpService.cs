using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Abstractions.Helpers;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Extension.Email.ExternalServices.Smtp
{
    public class SmtpService : BaseExternalService<SmtpExternalServiceData>, IExternalServiceDescriptor
    {
        public const string SmtpExternalServiceType = "SmtpExternalService";
        public override string ExternalServiceType => SmtpExternalServiceType;

        public string Name => "SMTP External Service";
        public string Description => "SMTP External Service to be able to send emails as an action";
        public string ViewPartial => "ViewSmtpExternalService";

        public Task<IActionResult> EditData(ExternalServiceData externalServiceData)
        {
            using (var scope = DependencyHelper.ServiceScopeFactory.CreateScope())
            {
                var identifier = externalServiceData.Id ?? $"new_{Guid.NewGuid()}";
                if (string.IsNullOrEmpty(externalServiceData.Id))
                {
                    var memoryCache = scope.ServiceProvider.GetService<IMemoryCache>();
                    memoryCache.Set(identifier, externalServiceData, new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(60)
                    });
                }

                return Task.FromResult<IActionResult>(new RedirectToActionResult(nameof(SmtpController.EditData),
                    "Smtp", new
                    {
                        identifier
                    }));
            }
        }

        public SmtpService() : base()
        {
        }

        public SmtpService(ExternalServiceData data) : base(data)
        {
        }

        public async Task SendEmail(MailMessage message)
        {
            var data = GetData();
            using (var client = new SmtpClient()
            {
                Port = data.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = data.Server,
                Credentials = new NetworkCredential(data.Username, data.Password),
                EnableSsl = data.SSL
            })
            {
                await client.SendMailAsync(message);
            }
        }
    }
}