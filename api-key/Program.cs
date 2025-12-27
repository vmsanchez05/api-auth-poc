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
    context.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey);

    if (extractedApiKey == "123abc")
    {
        await next.Invoke();
        return;
    }

    // Reject request
    context.Response.Headers["WWW-Authenticate"] = "API Key";
    context.Response.StatusCode = 401; // Unauthorized
});

app.MapGet("/", () => "Hello World!");

app.Run();
