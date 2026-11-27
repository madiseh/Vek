using Vek.Docs.Extensions;
using Vek.Docs.Models;
using Vek.Docs.Services;
using Vek.Docs.Server;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient<GitHubApiClient>();
builder.Services.TryAddDocsViewServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(sp =>
{
    var context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var client = new HttpClient { BaseAddress = new Uri($"{context!.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}") };

    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiCanonicalizationMiddleware>();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapGet("/profile", () => Results.NotFound());
app.MapGet("/archived-projects", () => Results.NotFound());
app.MapGet("/permissions", () => Results.NotFound());
app.MapGet("/features/break-points", () => Results.Redirect("/features/breakpoints", permanent: true));
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    // Warm up the documentation
    ApiDocumentation.GetType("VekAlert");
}

app.Run();
