var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/ping", () => Results.Text("pong")); // TODO: Тест, потом убрать 

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/userservice/swagger.json", "UserService");
    c.SwaggerEndpoint("/swagger/assessmentservice/swagger.json", "AssessmentService");
    c.SwaggerEndpoint("/swagger/authservice/swagger.json", "AuthService");
});

app.MapReverseProxy();

app.Run();