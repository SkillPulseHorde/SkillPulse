using System.Security.Claims;
using Common.Shared.Auth.Constants;
using Common.Shared.Auth.Extensions;
using Common.Shared.Auth.Models;

namespace Common.Shared.Auth;

public static class AccessPolicy
{
    public static bool CanAccess(ClaimsPrincipal? requestor, TargetInfo? target)
    {
        if (requestor == null || target == null)
            return false;

        var requesterInfo = requestor.GetRequesterInfo();

        if (requesterInfo.Id == target.Id)
            return true;

        if (Roles.OnlyHr.Contains(requesterInfo.Role))
            return true;

        if (target.ManagerId.HasValue && requesterInfo.Role == Role.DepartmentManager)
            return requesterInfo.Id == target.ManagerId;

        if (requesterInfo.Team != null && requesterInfo.Role == Role.ProductManager)
            return requesterInfo.Team == target.Team;

        return false;
    }
}