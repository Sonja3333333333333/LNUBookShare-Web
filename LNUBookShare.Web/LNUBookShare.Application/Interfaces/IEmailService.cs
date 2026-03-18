// <copyright file="IEmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LNUBookShare.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}