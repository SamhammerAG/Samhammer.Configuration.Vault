# Samhammer.Configuration.Vault

This library can be used if you want to load specific keys from vault. This is done by configuring the vault key as value.

If you just need a specific vault folder to be added as section to your appsettings you can use this library instead:
https://github.com/MrZoidberg/VaultSharp.Extensions.Configuration


## How to add this to your project:

- reference this package to your main project: https://www.nuget.org/packages/Samhammer.Configuration.Vault
- initialize in Program.cs
- add the health check to Program.cs (optional)
- use vault keys in your appsettings


## Example Program.cs:

```csharp
var builder = WebApplication.CreateBuilder(args);

var vaultUrl = "https://myHashicorpVault.com";
var authMethodInfo = new TokenAuthMethodInfo(token);
var options = new VaultOptions();

builder.Host.ConfigureAppConfiguration(cb => cb.AddVault(new Uri(vaultUri), authMethodInfo, options));
builder.Services.AddHealthChecks().AddVault(new Uri(vaultUri), authMethodInfo);
```
There is also an overload of AddVault where you can directly add the VaultSharp client.

All auth methods of VaultSharp are supported. See docs for further details: https://github.com/rajanadar/VaultSharp


## VaultOptions:

* VaultKeyPrefix: Used as value prefix and prefix for the internally created setting keys, that contain the vault keys. The default is "VaultKey--".
* ReloadInterval: If set, the reload from vault is enabled. Per default, the reload is disabled.
* OmitMissingSecrets: Per default, an exception is thrown if a settings key is missing in vault. If set to true the value of the setting will be left empty for missing vault secrets.


## Example appsettings configuration:
```json
"MyOptions": {
  "Username": "MyUserNameValue",
  "Password": "MyCustomPrefix--kv-v2/data/myproject/myfolder/mysecret/Password",
  "PasswordTwo": "MyCustomPrefix--myproject/myfolder/mysecret/Password"
},
```
The first part has to be the prefix configured in VaultOptions or if nothing set "VaultKey--". The vault keys can be added with "kv-v2/data/" or without.

Remark: Internally there will be added additional settings keys that hold the vault key. The setting key names are prefixed with the VaultKeyPrefix. e.g. MyCustomPrefix--PasswordTwo. These settings are below the same parent key.

## Watch for changes and get current value:

Use the IOptionsMonitor interface for that. IOptions is only initialized once.

You can find additional information here: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0#options-interfaces
