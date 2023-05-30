using System.Text;
using Chat.Areas.Api.Misc;
using Chat.Services;
using Chat.Services.Interface;
using Chat.Data;
using Chat.Domain;
using Chat.Areas.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var AllowAnyPortOnLocalhost = "_allowAnyPortOnLocalhost";

// Add services to the container.
builder.Services.AddLogging();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAnyPortOnLocalhost,
                      policy =>
                      {
                          policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                          policy.AllowAnyHeader();
                      });
});

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IUserChannelService, UserChannelService>();
builder.Services.AddScoped<IGroupChannelService, GroupChannelService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
                path.StartsWithSegments("/chat_hub"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IUserIdProvider, UsernameBasedUserIdProvider>();
builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseCors(AllowAnyPortOnLocalhost);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["chat_access_token"];
    if (!string.IsNullOrEmpty(token))
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    await next();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapAreaControllerRoute(
    "home_route",
    "WebPage",
    "/",
    new { controller = "Home", action = "Index" }
);

app.MapAreaControllerRoute(
    "login_route",
    "WebPage",
    "login/",
    new { controller = "Auth", action = "Login" }
);

app.MapAreaControllerRoute(
    "register_route",
    "WebPage",
    "register/",
    new { controller = "Auth", action = "Register" }
);


app.MapAreaControllerRoute(
    "chat_route",
    "WebPage",
    "chat/{*path}",
    new { controller = "Chat", action = "Index" }
);

app.MapHub<ChatHub>("/chat_hub");

app.Run();