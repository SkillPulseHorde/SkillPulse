namespace Common.Shared.Auth.Models;

public sealed record TargetInfo(
    Guid Id,
    string? Team,
    Guid? ManagerId,
    string? Role);