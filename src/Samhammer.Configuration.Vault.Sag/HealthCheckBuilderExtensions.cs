using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Samhammer.Configuration.Vault.Sag.Services;
using VaultSharp;

namespace Samhammer.Configuration.Vault.Sag
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddAuthenticatedVault(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            if (VaultAuthService.IsVaultDisabled())
            {
                return builder;
            }

            var vaultUrl = VaultAuthService.GetVaultUrl();
            var authMethodInfo = VaultAuthService.GetAuthMethodInfo();

            var clientSettings = new VaultClientSettings(vaultUrl, authMethodInfo)
            {
                UseVaultTokenHeaderInsteadOfAuthorizationHeader = true,
                VaultServiceTimeout = timeout,
            };

            var client = new VaultClient(clientSettings);

            return builder.AddVault(client, name, failureStatus, tags);
        }
    }
}
