//basic auth api

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    // Skip auth check for public endpoints
    if (context.Request.Path.StartsWithSegments("/public"))
    {
        await next();
        return;
    }

    var authHeader = context.Request.Headers["Authorization"].ToString();

    if (authHeader != null && authHeader.StartsWith("Basic "))
    {
        // Extract credentials
        var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
        var decodedUsernamePassword = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
        var username = decodedUsernamePassword.Split(':')[0];
        var password = decodedUsernamePassword.Split(':')[1];

        // Validate credentials
        if (username == "admin" && password == "password")
        {
            await next.Invoke();
            return;
        }
    }

    // Reject request
    context.Response.Headers["WWW-Authenticate"] = "Basic";
    context.Response.StatusCode = 401; // Unauthorized
});


app.MapGet("/public", () => "Hello World!");
app.MapGet("/secure", () => "super secret stuff");


app.Run();
