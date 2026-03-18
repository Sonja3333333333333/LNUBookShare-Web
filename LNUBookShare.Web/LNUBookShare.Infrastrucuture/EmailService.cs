// <copyright file="EmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Mail;
using LNUBookShare.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LNUBookShare.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? string.Empty;
        var senderPassword = _config["EmailSettings:SenderPassword"];
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var port = int.Parse(_config["EmailSettings:Port"] !);

        using var client = new SmtpClient(smtpServer, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
        };

        using var mailMessage = new MailMessage(senderEmail, email, subject, message)
        {
            IsBodyHtml = true,
        };

        await client.SendMailAsync(mailMessage);
    }
}