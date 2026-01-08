using CreditCard.Web.Components;
using CreditCard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HttpClient for API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7135";

builder.Services.AddHttpClient<CreditCardApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Configure HttpClient for Load Test Service (Singleton para mantener estado entre requests)
builder.Services.AddHttpClient("LoadTestClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddSingleton<LoadTestService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("LoadTestClient");
    var logger = sp.GetRequiredService<ILogger<LoadTestService>>();
    return new LoadTestService(httpClient, logger);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
