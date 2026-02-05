using OurRadio.Components;
using OurRadio.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=../ourradio.db"));

builder.Services.AddScoped<SongService>();
builder.Services.AddScoped<RadioService>();

var app = builder.Build();

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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
