using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Samhammer.Configuration.Vault.Health;
using VaultSharp;
using VaultSharp.V1.AuthMethods;

namespace Samhammer.Configuration.Vault
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddVault(
            this IHealthChecksBuilder builder,
            Uri vaultUri,
            IAuthMethodInfo credentials,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            var clientSettings = new VaultClientSettings(vaultUri.AbsoluteUri, credentials)
            {
                UseVaultTokenHeaderInsteadOfAuthorizationHeader = true,
                VaultServiceTimeout = timeout,
            };

            var client = new VaultClient(clientSettings);

            return builder.AddVault(client, name, failureStatus, tags);
        }

        public static IHealthChecksBuilder AddVault(
            this IHealthChecksBuilder builder,
            IVaultClient client,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
        {
            IHealthCheck Factory(IServiceProvider sp) => GetVaultHealthCheck(sp, client);
            return builder.Add(new HealthCheckRegistration(name ?? "vault", Factory, failureStatus, tags));
        }

        private static VaultHealthCheck GetVaultHealthCheck(IServiceProvider sp, IVaultClient client)
        {
            var logger = sp.GetRequiredService<ILogger<VaultHealthCheck>>();

            return new VaultHealthCheck(client, logger);
        }
    }
}
