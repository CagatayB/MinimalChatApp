using ChatApp.API;
using ChatApp.API.Hubs;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Database (using SQLite for easy portfolio cloning)
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();



// 2. Add CORS (Crucial for Frontend-Backend communication)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy.WithOrigins("http://localhost:5097") // Your Blazor URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// 3. Register SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("SignalRCors");

// 4. Map the SignalR Hub
app.MapHub<ChatHub>("/chathub");

// 5. Minimal API Endpoint: Get Chat History
app.MapGet("/api/messages/{userId}", async (string userId, ChatDbContext db) =>
{
    var messages = await db.Messages
        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
        .OrderBy(m => m.SentAt)
        .Take(50)
        .ToListAsync();

    return Results.Ok(messages);
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();
}


app.Run();

