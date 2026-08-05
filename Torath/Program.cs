using System.Text;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Torath;
using Torath.Middleware;
using Torath.Repositories;
using Torath.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers();

// ---> FIX 1: ADD CORS POLICY <---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Covers Vite and standard React ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 2. Configure Database Context (SQL Server)
builder.Services.AddDbContext<TorathDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Dependency Injection (Registering your Services)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IResearchPaperService, ResearchPaperService>();
builder.Services.AddScoped<IMagazineService, MagazineService>();
builder.Services.AddScoped<INewspaperIssueService, NewspaperIssueService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INewspaperService, NewspaperService>();
builder.Services.AddScoped<IMagazineIssueService, MagazineIssueService>();
builder.Services.AddScoped<IArticleService, ArticleService>();

// This single line registers the repository for EVERY entity automatically!
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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

// 6. Configure Elasticsearch
var elasticUri = builder.Configuration["Elasticsearch:Uri"];
var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
    .DefaultIndex("torath-searchable-content");

var elasticClient = new ElasticsearchClient(settings);
builder.Services.AddSingleton(elasticClient);
builder.Services.AddScoped<Torath.Services.IElasticSearchService, Torath.Services.ElasticSearchService>();

var app = builder.Build();

// --- THE MIDDLEWARE PIPELINE ---

// 1. Error Handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ---> CORS IS APPLIED HERE <---
app.UseCors("AllowFrontend");

// 3. Security (Order matters)
app.UseAuthentication();
app.UseAuthorization();

// 4. Map the API Routes
app.MapControllers();

// 5. Start the Engine
app.Run();