namespace Common.Shared.Auth.Models;

public sealed record RequesterInfo(
    Guid Id,
    string? Team,
    string? Role);