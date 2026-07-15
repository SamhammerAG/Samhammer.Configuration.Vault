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

            // Only a directly provided token can be stale on startup. Tokens from other auth methods
            // (e.g. kubernetes) are freshly minted on login, so there is nothing to verify up front.
            if (client.Settings.AuthMethodInfo?.AuthMethodType == AuthMethodType.Token)
            {
                VerifyToken(client);
            }

            configurationBuilder.Add(new ChainedVaultConfigurationSource(configurationBuilder.Build(), client, options));

            return configurationBuilder;
        }

        private static void VerifyToken(IVaultClient client)
        {
            try
            {
                // Looks up the current token to fail fast on an invalid or expired token
                // instead of surfacing an unclear error later when a secret is read.
                client.V1.Auth.Token.LookupSelfAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                throw new Exception("The vault token is invalid or expired. Please login with 'vault login'.", e);
            }
        }
    }
}
