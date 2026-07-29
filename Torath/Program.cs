using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Torath;
using Torath.Middleware;
using Torath.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();

// 2. Configure Database Context (SQL Server)
builder.Services.AddDbContext<TorathDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Dependency Injection (Registering your Services)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>(); // Added Categories!

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IResearchPaperService, ResearchPaperService>(); // If you built the implementation
builder.Services.AddScoped<IMagazineService, MagazineService>();
builder.Services.AddScoped<INewspaperIssueService, NewspaperIssueService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INewspaperService, NewspaperService>();
builder.Services.AddScoped<IMagazineIssueService, MagazineIssueService>();
// 4. Configure JWT Authentication
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

// 5. Configure Swagger (With JWT Bearer Support)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Torath API", Version = "v1" });

    // This creates the "Authorize" button in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token string below (without the quotes) to unlock secure endpoints."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- THE MIDDLEWARE PIPELINE ---

// 1. Error Handling (Catches crashes before they reach the user)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Swagger UI (Only in Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// 3. Security (These MUST be exactly in this order)
app.UseAuthentication(); // "Who are you? Do you have a token?"
app.UseAuthorization();  // "Are you allowed to be here?"

// 4. Map the API Routes
app.MapControllers();

// 5. Start the Engine
app.Run();