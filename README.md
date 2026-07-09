# Samhammer.Configuration.Vault

This library can be used if you want to load specific keys from vault. This is done by configuring the vault key as value.

If you just need a specific vault folder to be added as section to your appsettings you can use this library instead:
https://github.com/MrZoidberg/VaultSharp.Extensions.Configuration


## How to add this to your project:

- reference this package to your main project: https://www.nuget.org/packages/Samhammer.Configuration.Vault
- initialize in Program.cs
- add the health check to Program.cs (optional)
- use vault keys in your appsettings


### How to use in Program.cs

```csharp
var vaultUrl = "https://myHashicorpVault.com";
var authMethodInfo = new TokenAuthMethodInfo(token);
var options = new VaultOptions();

builder.Configuration.AddVault(new Uri(vaultUri), authMethodInfo, options);
builder.Services.AddHealthChecks().AddVault(new Uri(vaultUri), authMethodInfo);
```
There is also an overload of AddVault where you can directly add the VaultSharp client.

All auth methods of VaultSharp are supported. See docs for further details: https://github.com/rajanadar/VaultSharp


## VaultOptions:

* VaultKeyPrefix: Used as value prefix and prefix for the internally created setting keys, that contain the vault keys. The default is "VaultKey--".
* ReloadInterval: If set, the reload from vault is enabled. Per default, the reload is **disabled**.
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

This would also need ReloadInterval to be configured. But this reload can cause **heavy load** on vault if you have a lot of settings. So use it with care.

You can find additional information here: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0#options-interfaces


## Authenticated Vault (Samhammer.Configuration.Vault.Sag)

This is an internally used convenience library to add Samhammer.Configuration.Vault.

It does the following in addition:
* Locally: Uses the url from the VAULT_ADDR environment variable and the token returned by the Vault CLI (`vault print token`)
* Kubernetes: Does a kubernetes role auth

### Prerequirements

#### Locally

The HashiCorp Vault CLI has to be installed and you have to be logged in (`vault login`): https://developer.hashicorp.com/vault/docs/commands

Use following environment variables for configuration:
* VAULT_ADDR: With the url to vault (required)

#### In the cluster

Use following environment variables for configuration:
* VaultUrl: With the url to vault (required)
* VaultKubernetesRole: The vault role of the application (required)
* VaultDisabled: When true skips adding vault and healthchecks. (optional)

## How to add this to your project:

- reference this package to your main project: https://www.nuget.org/packages/Samhammer.Configuration.Vault.Sag

### How to use in Program.cs

```csharp
builder.Configuration
        .AddAuthenticatedVault(vaultOptions);

builder.Services
        .AddHealthChecks()
        .AddAuthenticatedVault();
```

See the [VaultOptions](#vaultoptions) section above for the available vault options.
