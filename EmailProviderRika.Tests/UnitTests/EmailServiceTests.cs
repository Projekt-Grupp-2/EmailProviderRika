

using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProviderRika.Models;
using EmailProviderRika.Services;
using EmailProviderRika.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace EmailProviderRika.Tests.UnitTests;

public class EmailServiceTests()
{

    [Fact]
    public void UnPackEmailRequest_ValidMessage_ReturnsSignUpRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();

        var signUpRequest = new SignUpRequest 
        { 
            Email = "Test",
            Name = "Test"
        };
        var messageContent = JsonConvert.SerializeObject(signUpRequest);

        ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(messageContent),
            messageId: "",
            partitionKey: "illustrative-partitionKey",
            correlationId: "illustrative-correlationId",
            contentType: "illustrative-contentType",
            replyTo: "illustrative-replyTo"
            // ...
            );


        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        // Act
        var result = emailService.UnPackEmailRequest(message);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void UnPackEmailRequest_InValidMessage_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();


        ServiceBusReceivedMessage message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(""),
            messageId: null,
            partitionKey: null,
            correlationId: null,
            contentType: null,
            replyTo: null
            // ...
            );


        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        // Act
        var result = emailService.UnPackEmailRequest(message);

        // Assert
        Assert.Null(result);
    }


    [Fact]

    public void FormatMessage_SignUpRequestIsValid_ReturnEmailRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();

        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        var signUpRequest = new SignUpRequest
        {
            Email = "Hej",
            Name = "Test"
        };


        // Act

        var result = emailService.FormatMessage(signUpRequest);

        //Assert
        Assert.NotNull(result);
    }

    [Fact]

    public void FormatMessage_SignUpRequestIsNotValid_ReturnEmailRequest()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();

        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        var signUpRequest = new SignUpRequest
        {
        };


        // Act

        var result = emailService.FormatMessage(signUpRequest);

        //Assert
        Assert.Null(result);
    }

    [Fact]

    public void SendMessage_EmailRequestIsValid_ReturnTrueIfEmailClientSends()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();


        var mockOperationResult = new Mock<EmailSendOperation>();
        mockOperationResult.Setup(r => r.HasCompleted).Returns(true);

        // Setup för att mocka EmailClient.Send
        mockEmailClient.Setup(client => client.Send(
                WaitUntil.Completed,   
                It.IsAny<string>(),     
                It.IsAny<string>(),     
                It.IsAny<string>(),     
                It.IsAny<string>(),     
                It.IsAny<string>(),     
                It.IsAny<CancellationToken>()
            ))
            .Returns(mockOperationResult.Object); 

        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        var signUpRequest = new SignUpRequest
        {
            Email = "carllindqvist93@gmail.com",
            Name = "Test"
        };

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


        // Act

        var result = emailService.SendEmail(emailRequest);

        //Assert
        Assert.True(result);
    }

    [Fact]

    public void SendMessage_EmailRequestIsNotValid_ReturnFalseIfEmailClientDontSend()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmailService>>();

        var mockEmailClient = new Mock<EmailClient>();


        var mockOperationResult = new Mock<EmailSendOperation>();
        mockOperationResult.Setup(r => r.HasCompleted).Returns(false);

        // Setup för att mocka EmailClient.Send
        mockEmailClient.Setup(client => client.Send(
                WaitUntil.Completed,   // Placeholder för WaitUntil.Completed
                It.IsAny<string>(),     // senderAddress
                It.IsAny<string>(),     // recipientAddress
                It.IsAny<string>(),     // subject
                It.IsAny<string>(),     // htmlContent
                It.IsAny<string>(),     // PlainText
                It.IsAny<CancellationToken>()
            ))
            .Returns(mockOperationResult.Object); 

        var emailService = new EmailService(mockLogger.Object, mockEmailClient.Object);

        var signUpRequest = new SignUpRequest
        {
            Email = null!,
            Name = null!
        };

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


        // Act

        var result = emailService.SendEmail(emailRequest);

        //Assert
        Assert.False(result);
    }
}

