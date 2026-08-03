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
using Elastic.Clients.Elasticsearch;

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




// 1. Grab the URL from appsettings
var elasticUri = builder.Configuration["Elasticsearch:Uri"];

// 2. Configure the client
var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
    .DefaultIndex("torath-searchable-content"); // The default index we created

var elasticClient = new ElasticsearchClient(settings);

// 3. Register the Client and your new Service into the Dependency Injection container
builder.Services.AddSingleton(elasticClient);
builder.Services.AddScoped<Torath.Services.IElasticSearchService, Torath.Services.ElasticSearchService>();

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