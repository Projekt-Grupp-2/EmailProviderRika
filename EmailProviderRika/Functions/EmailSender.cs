using Azure.Messaging.ServiceBus;
using EmailProviderRika.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailProviderRika.Functions;

public class EmailSender(ILogger<EmailSender> logger, EmailService emailService)
{
    private readonly ILogger<EmailSender> _logger = logger;
    private readonly EmailService _emailService = emailService;

    [Function(nameof(EmailSender))]
    public async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var signUpRequest = _emailService.UnPackEmailRequest(message);
            if (signUpRequest != null && !string.IsNullOrEmpty(signUpRequest.Email))
            {
                var emailRequest = _emailService.FormatMessage(signUpRequest);

                if (emailRequest != null)
                {
                    if (emailService.SendEmail(emailRequest))
                    {
                        await messageActions.CompleteMessageAsync(message);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run  :: {ex.Message}");
        }
    }


}

