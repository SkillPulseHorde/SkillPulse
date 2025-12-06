using Microsoft.Extensions.Options;
using RecommendationService.Infrastructure.Http.ServiceClientOptions;
using RecommendationService.Infrastructure.Http.ServiceClients;
using RecommendationService.Application.ServiceClientsAbstract;

namespace RecommendationService.Api.Extensions.ServiceRegistration;

public static class UserServiceRegistration
{
    public static IServiceCollection AddUserServiceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<UserServiceOptions>()
            .Bind(configuration.GetSection("Services:UserService"))
            .ValidateDataAnnotations()
            .Validate(options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute),
                "(UserService) BaseUrl должен быть валидным URI")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InternalToken),
                "(UserService) InternalToken должен быть передан")
            .ValidateOnStart();

        services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}