using DotNetEnv;
using EBallotApi.Middleware;
using EBallotApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Data;
using System.Text;
using System.Threading.RateLimiting;
using static Dapper.SqlMapper;
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// --- CORS setup ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});




/// ---JWT Settings from Environment Variables ---
/// 
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? throw new Exception("JWT_SECRET environment variable is not set.");
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "EBallotIssuer";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "EBallotAudience";

var key = Encoding.UTF8.GetBytes(secretKey);

// --- JWT Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // True for production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

//Global Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Use IP address as key for simplicity
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,                   // allow 100 requests
            Window = TimeSpan.FromMinutes(1),    // per 1 minute
            QueueLimit = 0,                       
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 3;                 // 3 login attempts
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueLimit = 0;
    });

    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.");
    };

});






builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IVoterService, VoterService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConstituencyService,ConstituencyService>();
builder.Services.AddScoped<IAdminDashboardService,AdminDashboardService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EBallot API", Version = "v1" });

    // Added JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\nExample: \"Bearer eyJhbGciOiJIUzI1NiIs...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Global Middleware for Exception Handling
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// --- Enable JWT Authentication ---
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
