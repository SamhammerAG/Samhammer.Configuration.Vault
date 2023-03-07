using System;
using Microsoft.Extensions.Configuration;
using Samhammer.Configuration.Vault.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace Samhammer.Configuration.Vault
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddVault(this IConfigurationBuilder configurationBuilder, Uri vaultUri, IAuthMethodInfo credentials, VaultOptions options = null)
        {
            var clientSettings = new VaultClientSettings(vaultUri.AbsoluteUri, credentials)
            {
                UseVaultTokenHeaderInsteadOfAuthorizationHeader = true,
            };

            var client = new VaultClient(clientSettings);

            return configurationBuilder.AddVault(client, options);
        }

        public static IConfigurationBuilder AddVault(this IConfigurationBuilder configurationBuilder, IVaultClient client, VaultOptions options = null)
        {
            if (options == null)
            {
                options = new VaultOptions { VaultKeyPrefix = VaultOptions.DefaultVaultKeyPrefix };
            }

            if (string.IsNullOrEmpty(options.VaultKeyPrefix))
            {
                options.VaultKeyPrefix = VaultOptions.DefaultVaultKeyPrefix;
            }

            if (options.VaultKeyPrefix.Contains(ConfigurationPath.KeyDelimiter))
            {
                throw new ArgumentException($"Don't use '{ConfigurationPath.KeyDelimiter}' (dotnet section delimiter) as part of the prefix");
            }

            configurationBuilder.Add(new ChainedVaultConfigurationSource(configurationBuilder.Build(), client, options));

            return configurationBuilder;
        }
    }
}
