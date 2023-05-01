var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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
    "chat/",
    new { controller = "Chat", action = "Index" }
);

app.MapBlazorHub();

app.Run();