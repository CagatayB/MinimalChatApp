using ChatApp.API.Hubs;
using ChatApp.Application.Services;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration & JWT Setup ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- Database & Infrastructure ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=chat.db";

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddScoped<IChatValidationService, ChatValidationService>();

// --- Real-time & Middleware ---
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy.WithOrigins("http://localhost:5097")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("SignalRCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");

// --- Minimal API Endpoints ---

// Get Chat History
app.MapGet("/api/messages/{userId}/{contactId}", async (string userId, string contactId, ChatDbContext db) =>
{
    return await db.Messages
        .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                    (m.SenderId == contactId && m.ReceiverId == userId))
        .OrderBy(m => m.SentAt)
        .Take(50)
        .ToListAsync() is List<ChatMessage> history
            ? Results.Ok(history)
            : Results.NotFound();
});

// Login
app.MapPost("/api/auth/login", (LoginRequest request) => {
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, request.Username) }),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
});

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// DTO for clean login
public record LoginRequest(string Username);