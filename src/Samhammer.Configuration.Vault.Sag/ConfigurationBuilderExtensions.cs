using System;
using Microsoft.Extensions.Configuration;
using Samhammer.Configuration.Vault.Sag.Services;

namespace Samhammer.Configuration.Vault.Sag
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAuthenticatedVault(this IConfigurationBuilder configurationBuilder, VaultOptions options)
        {
            if (VaultAuthService.IsVaultDisabled())
            {
                return configurationBuilder;
            }

            var vaultUrl = VaultAuthService.GetVaultUrl();
            var authMethodInfo = VaultAuthService.GetAuthMethodInfo();

            return configurationBuilder.AddVault(new Uri(vaultUrl), authMethodInfo, options);
        }
    }
}
