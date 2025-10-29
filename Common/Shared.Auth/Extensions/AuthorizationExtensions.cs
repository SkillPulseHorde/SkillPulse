﻿using Common.Shared.Auth.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Shared.Auth.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddRoleBasedAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Политика: только HR
            options.AddPolicy("HROnly", policy =>
                policy.RequireRole(Roles.OnlyHr));

            // Политика: только руководители (Department + Product)
            options.AddPolicy("ManagersOnly", policy =>
                policy.RequireRole(Roles.AllManagers));

            // Политика: HR и руководители
            options.AddPolicy("HRAndManagers", policy =>
                policy.RequireRole(Roles.AllManagersAndHr));

            // Политика: любой авторизованный пользователь
            options.AddPolicy("Authenticated", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}