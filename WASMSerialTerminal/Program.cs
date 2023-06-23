using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WASMSerialTerminal;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ISerialService, SerialService>();
builder.Services.AddScoped<Radzen.DialogService>();

WebAssemblyHost host = builder.Build();

// Load USB devices from JSON file
var serialService = host.Services.GetRequiredService<ISerialService>();
await serialService.InitializeDevices();

await host.RunAsync();