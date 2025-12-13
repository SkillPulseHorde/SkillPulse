using System.Security.Claims;
using Common.Shared.Auth.Constants;
using Common.Shared.Auth.Extensions;
using Common.Shared.Auth.Models;

namespace Common.Shared.Auth;

/// <summary>
/// Provides methods for determining whether a requesting user has access to a target user's information,
/// based on role, team, and organizational hierarchy.
/// </summary>
public static class AccessPolicy
{
    /// <summary>
    /// Determines whether the requesting user is authorized to access the target user's information.
    /// </summary>
    /// <param name="requestor">The user making the access request, represented as a <see cref="ClaimsPrincipal"/>.</param>
    /// <param name="target">The target user whose information is being accessed, represented as a <see cref="TargetInfo"/>.</param>
    /// <returns>
    /// <c>true</c> if the requestor is authorized to access the target; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Access is granted if any of the following conditions are met:
    /// <list type="bullet">
    ///   <item>
    ///     <description>The requestor is accessing their own information (self-access).</description>
    ///   </item>
    ///   <item>
    ///     <description>The requestor has an HR role (as defined in <see cref="Roles.OnlyHr"/>).</description>
    ///   </item>
    ///   <item>
    ///     <description>The requestor is a Department Manager and is the manager of the target user.</description>
    ///   </item>
    ///   <item>
    ///     <description>The requestor is a Product Manager and is in the same team as the target user.</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static bool CanAccess(ClaimsPrincipal? requestor, TargetInfo? target)
    {
        if (requestor == null || target == null)
            return false;

        var requesterInfo = requestor.GetRequesterInfo();

        if (requesterInfo.Id == target.Id)
            return true;

        if (requesterInfo.Role != null && Roles.OnlyHr.Contains(requesterInfo.Role))
            return true;

        if (target.ManagerId.HasValue && requesterInfo.Role == Role.DepartmentManager)
            return requesterInfo.Id == target.ManagerId;

        if (requesterInfo.Team != null && requesterInfo.Role == Role.ProductManager)
            return requesterInfo.Team == target.Team;

        return false;
    }
}