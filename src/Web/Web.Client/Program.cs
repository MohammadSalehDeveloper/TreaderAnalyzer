using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web.Client;
using Web.Client.Services.Account;
using Web.Client.Services.Admin;
using Web.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5065/";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IAuthSession, AuthSession>();
builder.Services.AddScoped<IAccountApiClient, AccountApiClient>();
builder.Services.AddScoped<IAdminUsersApiClient, AdminUsersApiClient>();

var host = builder.Build();

var authSession = host.Services.GetRequiredService<IAuthSession>();
await authSession.InitializeAsync();

await host.RunAsync();
