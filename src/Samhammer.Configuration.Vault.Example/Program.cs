using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Samhammer.Configuration.Vault;
using Samhammer.Configuration.Vault.Example;
using Samhammer.Configuration.Vault.Sag;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });

// AddAuthenticatedVault gets the vault url and credentials on its own (kubernetes / sagctl).
builder.Configuration.AddAuthenticatedVault(new VaultOptions());
builder.Services.Configure<MyOptions>(builder.Configuration.GetSection("MyOptions"));

using var host = builder.Build();

// Resolve the configured "VaultKey--" values from vault.
var myOptions = host.Services.GetRequiredService<IOptions<MyOptions>>().Value;

Console.WriteLine($"Log-Username: {myOptions.UserName}");
