using FMS_server.Environment.Register;
using FMS_server.Layers.ContextLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
//using payday_server.Model.Middleware;
using FMS_server.Shared;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register AppDBContext first
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Local")));

// Then the rest of your custom configurations
builder.Services.ConnectionConfigure();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureAuthentication();

builder.Services.AddHttpClient("FastAPIClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/api/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("mycors",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(30))
    );
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Payday.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureProcessor();
builder.Services.ConfigureSwaggerGeneration();
builder.Services.AddDistributedMemoryCache();

builder.Services.ConfigureSignalR();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("mycors");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();

// Map SignalR hub
app.MapHub<NotificationsHub>("/notifications");

app.Run();
