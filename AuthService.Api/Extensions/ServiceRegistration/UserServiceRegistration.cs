using AuthService.Application.ServiceClientsAbstract;
using AuthService.Infrastructure.Http.ServiceClientOptions;
using AuthService.Infrastructure.Http.ServiceClients;
using Microsoft.Extensions.Options;

namespace AuthService.Api.Extensions.ServiceRegistration;

public static class UserServiceRegistration
{
    public static IServiceCollection AddUserServiceClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<UserServiceOptions>()
            .Bind(configuration.GetSection("Services:UserService"))
            .ValidateDataAnnotations()
            .Validate(options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute),
                "BaseUrl должен быть валидным URI")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InternalToken),
                "InternalToken должен быть передан")
            .ValidateOnStart();

        services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}