using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/ping", () => Results.Text("pong")); // TODO: Тест, потом убрать 

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/userservice/swagger.json", "UserService");
    c.SwaggerEndpoint("/swagger/authservice/swagger.json", "AuthService");
});

app.MapReverseProxy();

app.Run();