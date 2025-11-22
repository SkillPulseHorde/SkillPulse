var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(option  =>
{
    option.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "null")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/userservice/swagger.json", "UserService");
    c.SwaggerEndpoint("/swagger/assessmentservice/swagger.json", "AssessmentService");
    c.SwaggerEndpoint("/swagger/authservice/swagger.json", "AuthService");
});


app.MapReverseProxy();

app.Run();