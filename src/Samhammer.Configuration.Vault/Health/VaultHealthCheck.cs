using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace Samhammer.Configuration.Vault.Health
{
    public class VaultHealthCheck : IHealthCheck
    {
        private readonly IVaultClient _client;
        private readonly ILogger<VaultHealthCheck> _logger;

        public VaultHealthCheck(IVaultClient client, ILogger<VaultHealthCheck> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isHealthy = await IsVaultHealthy();

            return isHealthy ? HealthCheckResult.Healthy("ok") : HealthCheckResult.Unhealthy($"vault is not healthy");
        }

        public async Task<bool> IsVaultHealthy()
        {
            try
            {
                var status = await _client.V1.System.GetHealthStatusAsync();
                _logger.LogDebug("Vault health check returned status code {StatusCode} and init state {IsInitialized}", status.HttpStatusCode, status.Initialized);

                return status.HttpStatusCode == (int)HttpStatusCode.OK && status.Initialized;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Vault health check failed with exception");
                return false;
            }
        }
    }
}
