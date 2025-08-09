using EventApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EventApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using EventApi.Interfaces;
using EventApi.Repository;
using EventApi.Services;
using Hangfire;
using Microsoft.AspNetCore.Builder;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var firebaseProjectJsonPath = builder.Configuration["Firebase:ProjectJsonPath"];
if (string.IsNullOrEmpty(firebaseProjectJsonPath))
{
    throw new ArgumentNullException("Firebase:ProjectJsonPath", "Firebase project JSON path is not configured.");
}

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(firebaseProjectJsonPath)
});


builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDBContext>();

builder.Services.AddAuthentication(options =>
{
    // Set the default schemes to JwtBearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://securetoken.google.com/qrplatform-1d636";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://securetoken.google.com/qrplatform-1d636",
        ValidateAudience = true,
        ValidAudience = "qrplatform-1d636",
        ValidateLifetime = true
    };
});


builder.Services.AddAuthorization(options =>
{
    // This creates a default policy that all [Authorize] attributes will use
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser() // The user must be authenticated
        .Build();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

});

var MyAllowedOrigins = "_myAllowedOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowedOrigins,
        policy =>
        {
            // Be EXTREMELY specific. Add the exact origins.
            policy.WithOrigins(
                    "http://localhost:3000",      // Your friend's dev server on his PC
                    "http://127.0.0.1:3000",     // Also good to include
                    "http://localhost:5500",      // Your local test page
                    "http://127.0.0.1:5500",      // Your local test page
                    "https://localhost:7093",
                    "https://mk25szk5-7093.inc1.devtunnels.ms"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IFileHandlingService, FileHandlingService>();
builder.Services.AddScoped<IImageGenerationService, ImageGenerationService>();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowedOrigins);


app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard(); 

app.MapControllers();

app.Run();
