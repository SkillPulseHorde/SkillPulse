﻿namespace AuthService.Application.Models;

public record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty; 
    public string RefreshToken { get; init; } = string.Empty;
}