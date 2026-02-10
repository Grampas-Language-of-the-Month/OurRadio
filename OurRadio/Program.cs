using OurRadio.Components;
using OurRadio.Data;
using OurRadio.Hubs;
using OurRadio.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.Discord;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSignalR();
builder.Services.AddSingleton<RadioClockService>();

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=../ourradio.db"));

builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<RadioService>();
builder.Services.AddScoped<AuthGuard>();

// Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddDiscord(options =>
{
    options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
    options.CallbackPath = "/signin-discord";
    options.SaveTokens = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    var unsafeUploadsPath = Path.Combine(Directory.GetParent(app.Environment.ContentRootPath)!.FullName,
                app.Environment.EnvironmentName, "unsafe_uploads");

    // log the path for debugging purposes
    app.Logger.LogInformation($"Serving static files from: {unsafeUploadsPath}");

    var contentTypeProvider = new FileExtensionContentTypeProvider();
    contentTypeProvider.Mappings[".flac"] = "audio/flac";
    contentTypeProvider.Mappings[".mp3"] = "audio/mpeg";
    contentTypeProvider.Mappings[".ogg"] = "audio/ogg";

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(unsafeUploadsPath),
        RequestPath = "/uploads",
        ContentTypeProvider = contentTypeProvider
    });
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<RadioHub>("/hubs/radio");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/login", async context =>
{
    await context.ChallengeAsync(DiscordAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Run();
