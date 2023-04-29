using System.Text;
using Chat.Server.Configs;
using Chat.Server.Data;
using Chat.Server.Models;
using Chat.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Services.AddLogging();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add database 
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add api endpoints
builder.Services.AddControllers();

// Add Swagger/OpenAPI interface
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configure auth options
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please input a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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


// Add services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IConnectionService, ConnectionService>();

// Config authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var secretKey = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");
    if (secretKey == null)
        throw new Exception("Environment variable DEV_SECRET_KEY is not set");

    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "chat",
        ValidAudience = "chat",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chat"))
                context.Token = accessToken;
            return Task.CompletedTask;
        },
    };
});
builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IUserIdProvider, UsernameBasedUserIdProvider>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.Run();