using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using SurveyApp.Repo;
using SurveyApp.Data;
using SurveyApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(Local);Database=VLDev;Integrated Security=True;Connect Timeout=360000;TrustServerCertificate=True"));

// === Session Configuration ===
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === CORS Configuration for Mobile App ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileAppPolicy", policy =>
    {
        policy.AllowAnyOrigin()  // Allow mobile apps from any origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition", "Content-Length");
    });
    
    options.AddPolicy("WebAppPolicy", policy =>
    {
        policy.WithOrigins(
                "https://survey.vluccc.com:91",
                "http://localhost:5000",
                "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// === JWT Authentication Configuration ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "cca028cb830260edae80187d8dcb6755";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Allow HTTP in development
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "SurveyApp",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "SurveyAppMobile",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
    };
    
    // Handle authentication events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Skip default behavior for API endpoints
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Unauthorized. Please provide a valid token.",
                    timestamp = DateTime.UtcNow
                });
                return context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// === Swagger/OpenAPI Configuration ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Survey Application API",
        Version = "v1",
        Description = "RESTful API for Survey Application - Mobile App Integration",
        Contact = new OpenApiContact
        {
            Name = "VL Survey Team",
            Email = "support@vluccc.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: {your_token}"
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

    // Enable XML documentation (optional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// === Core Services ===
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure Cloudinary settings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Configure Location API Service
builder.Services.AddHttpClient<SurveyApp.Services.ILocationApiService, SurveyApp.Services.LocationApiService>();

// === Repository Services ===
builder.Services.AddScoped<ICommonUtil, CommonUtil>();
builder.Services.AddScoped<IAdmin, AdminRepo>();
builder.Services.AddScoped<ISurvey, SurveyRepo>();
builder.Services.AddScoped<ISurveyLocation, SurveyLocationRepo>();
builder.Services.AddScoped<ISurveyLocationStatus, SurveyLocationStatusRepo>();
builder.Services.AddScoped<ISurveySubmission, SurveySubmissionRepo>();
builder.Services.AddScoped<IClientMaster, ClientMasterRepo>();
builder.Services.AddScoped<IEmpMaster, EmpMasterRepo>();
builder.Services.AddScoped<ISurveyCamRemarks, SurveyCamRemarksRepo>();
builder.Services.AddScoped<IReportOTP, ReportOTPRepo>();
builder.Services.AddScoped<ISurveyRevision, SurveyRevisionRepo>();

// === JWT Service ===
builder.Services.AddScoped<IJwtService, JwtService>();

// Password hashing service (singleton - no state)
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// === Third-party License Configuration ===
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
ExcelPackage.License.SetNonCommercialOrganization("ABTMS");

// Add controllers with views (for existing MVC) and API controllers
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization for API responses
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// === Configure the HTTP request pipeline ===

// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Survey API v1");
    options.RoutePrefix = "api-docs"; // Access at /api-docs
    options.DocumentTitle = "Survey API Documentation";
    options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Enable CORS - must be before UseRouting
app.UseCors("MobileAppPolicy");

app.UseSession();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization - order matters!
app.UseAuthentication();
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserLogin}/{action=Index}/{id?}");

// Map API controllers
app.MapControllers();

app.Run();
