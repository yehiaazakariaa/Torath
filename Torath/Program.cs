using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Torath;
using Torath.Middleware;
using Torath.Services; // Ensure this is here

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<TorathDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 1. DEPENDENCY INJECTION REGISTRATION ---
// Tells the app: "Whenever a controller asks for IAuthService, give them AuthService"
builder.Services.AddScoped<IAuthService, AuthService>();

// --- 2. JWT AUTHENTICATION CONFIGURATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// --- 3. MIDDLEWARE PIPELINE ---
// These MUST go exactly here, before MapControllers!
app.UseAuthentication(); // Checks if the token is valid
app.UseAuthorization();  // Checks if the user has permission to view the route

app.MapControllers();

app.Run();