using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using PasskeyHackathon2._0.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// Add Domains Here
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200","https://passkey-auth-woad.vercel.app/")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register repository
builder.Services.AddScoped<PasskeyHackathon2._0.Repository.IPasskeyRepository, PasskeyHackathon2._0.Repository.PasskeyRepository>();

// Configure Fido2 on DI 
var fidoSection = builder.Configuration.GetSection("Fido2");
var fidoConfig = fidoSection.Get<Fido2Configuration>() ?? new Fido2Configuration
{
    ServerDomain = builder.Configuration["Fido2:ServerDomain"] ?? "localhost",
    ServerName = builder.Configuration["Fido2:ServerName"] ?? "PasskeyHackathon",
    TimestampDriftTolerance = int.TryParse(builder.Configuration["Fido2:TimestampDriftTolerance"], out var tdt) ? tdt : 300000
};

builder.Services.AddSingleton(fidoConfig);
builder.Services.AddSingleton<Fido2>(sp => new Fido2(sp.GetRequiredService<Fido2Configuration>()));
builder.Services.AddSingleton<IFido2>(sp => sp.GetRequiredService<Fido2>());
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<Fido2>(sp =>
{
    return new Fido2(new Fido2Configuration
    {
        ServerDomain = "localhost",
        ServerName = "PasskeyApp",
        Origins = new HashSet<string>
        {
            "http://localhost:4200",
            "https://localhost:4200" , 
            "https://passkey-auth-woad.vercel.app"
        }
    });
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("DefaultCorsPolicy");
app.UseSession();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
