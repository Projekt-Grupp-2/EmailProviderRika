﻿using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProviderRika.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EmailProviderRika.Services;


public class EmailService(ILogger<EmailService> logger, EmailClient emailClient)
{

    private readonly ILogger<EmailService> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;

    public SignUpRequest UnPackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<SignUpRequest>(message.Body.ToString());
            if (request != null)
                return request;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnPackEmailRequest  :: {ex.Message}");
        }

        return null!;
    }

    public EmailRequest FormatMessage(SignUpRequest signUpRequest)
    {
        try
        {
            if (!string.IsNullOrEmpty(signUpRequest.Email))
            {
                var emailRequest = new EmailRequest()
                {
                    To = signUpRequest.Email,
                    Subject = $"Welcome to Rika!",
                    HtmlBody = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                              <meta charset=""UTF-8"">
                              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                              <style>
                                body {{
                                  font-family: Arial, sans-serif;
                                  margin: 0;
                                  padding: 0;
                                  background-color: #f4f4f4;
                                }}
                                .email-container {{
                                  max-width: 600px;
                                  margin: auto;
                                  background-color: #ffffff;
                                  padding: 20px;
                                  border-radius: 8px;
                                  box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                  background-color: #4CAF50;
                                  padding: 20px;
                                  text-align: center;
                                  color: #ffffff;
                                  border-radius: 8px 8px 0 0;
                                }}
                                .header h1 {{
                                  margin: 0;
                                  font-size: 24px;
                                }}
                                .content {{
                                  padding: 20px;
                                  text-align: left;
                                  color: #333333;
                                }}
                                .content p {{
                                  line-height: 1.6;
                                  margin: 0 0 15px;
                                }}
                                .content a {{
                                  color: #4CAF50;
                                  text-decoration: none;
                                  font-weight: bold;
                                }}
                                .footer {{
                                  padding: 20px;
                                  text-align: center;
                                  font-size: 12px;
                                  color: #666666;
                                }}
                              </style>
                            </head>
                            <body>
                              <div class=""email-container"">
                                <!-- Header -->
                                <div class=""header"">
                                  <h1>Welcome to Rika!</h1>
                                </div>
    
                                <!-- Content -->
                                <div class=""content"">
                                  <p>Hi {signUpRequest.Name},</p>
                                  <p>Thank you for signing up with us! We’re excited to have you on board. At Rika, we strive to provide the best experience for our users, and we’re glad to have you as part of our community.</p>
                                  <p>We look forward to seeing you around!</p>
                                  <p>Best regards,<br>Rika Team</p>
                                </div>
    
                                <!-- Footer -->
                                <div class=""footer"">
                                  <p>&copy; 2024 Rika. All rights reserved.</p>
                                  <p>You received this email because you signed up for an account with us. If you didn’t sign up, please ignore this email.</p>
                                </div>
                              </div>
                            </body>
                            </html>
                      ",
                    PlainText = $"Hi {signUpRequest.Name},\r\n\r\nThank you for signing up with us! We’re excited to have you in our community at Rika."
                };


                return emailRequest;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.FormatMessage  :: {ex.Message}");
        }
        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,
                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.To,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.HtmlBody,
                plainTextContent: emailRequest.PlainText
                );

            if (result.HasCompleted)
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.SendEmailAsync  :: {ex.Message}");
        }

        return false;
    }

}
