using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var secretKey = "supersecretkey12345forjwttokensanditisreallyLongtomeetrequirements";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/public", () => "This is a public endpoint accessible to everyone.");

app.MapPost("/login", (UserCredentials creds) =>
{
    if (creds.Username == "user1" && creds.Password == "pass123")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, creds.Username),
                new Claim(ClaimTypes.Role, "Admin") // Optional: Add roles/claims
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return Results.Ok(new { token = jwtToken });
    }

    return Results.Unauthorized();
});

app.MapGet("/secure", [Authorize] (ClaimsPrincipal user) =>
{
    var username = user.Identity?.Name;
    return $"Welcome {username}, you are authenticated!";
});

app.MapGet("/admin", [Authorize(Roles = "Admin")] (ClaimsPrincipal user) =>
{
    var username = user.Identity?.Name;
    return $"Welcome {username}, you are an Admin!";
});


app.Run();


record UserCredentials(string Username, string Password);
