using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Infrastructure.Http.ServiceClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace AssessmentService.Infrastructure.Http.ServiceCollectionExtensions;

public static class UserServiceRegistration
{
    public static IServiceCollection AddUserServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<UserServiceOptions>()
            .Bind(configuration.GetSection("Services:UserService"))
            .ValidateDataAnnotations()
            .Validate(o => Uri.IsWellFormedUriString(o.BaseUrl, UriKind.Absolute), "BaseUrl must be a valid URI")
            .ValidateOnStart();

        services.AddHttpClient<IUserServiceClient, UserServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}