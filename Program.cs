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
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using MailerSendNetCore.Common.Extensions;
using EventApi.ExeptionHandling;
using EventApi.Filters;
using FluentValidation;
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
// something
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
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
            policy.WithOrigins(
                    "http://localhost:3000",      
                    "http://127.0.0.1:3000",     
                    "http://localhost:5500",      
                    "http://127.0.0.1:5500",      
                    "https://localhost:7093",
                    "https://mk25szk5-7093.inc1.devtunnels.ms",
                    "https://q-rplatform.vercel.app"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => 
    options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();


builder.Services.AddExceptionHandler<GlobalExeptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IFileHandlingService, FileHandlingService>();
builder.Services.AddScoped<IImageGenerationService, ImageGenerationService>();
builder.Services.AddScoped<IEmailSevice, EmailService>();
builder.Services.AddScoped<IAccountRepository,AccountRepository>();
builder.Services.AddScoped<IFirebaseAdminService,FirebaseAdminService>();
builder.Services.AddScoped<IMessageService,WhatsAppService>();
builder.Services.AddScoped<IFontManagerService,FontManagerService>();

//builder.Services.AddScoped<SubscriptionCheckAttribute>();

builder.Services.AddMailerSendEmailClient(options =>
{
    options.ApiToken = builder.Configuration.GetValue<string>("EmailSettings:MailerSendApiKey");    
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowedOrigins);


app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard(); 

app.MapControllers();

app.Run();
