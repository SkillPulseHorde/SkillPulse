var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/users", () => new[] { "Alice", "Bob", "Charlie" });

app.Run();