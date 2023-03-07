using System;
using Microsoft.Extensions.Configuration;
using Samhammer.Configuration.Vault.Services;
using VaultSharp;

namespace Samhammer.Configuration.Vault.Configuration
{
    internal class ChainedVaultConfigurationSource : IConfigurationSource
    {
        private readonly IConfiguration _configuration;
        private readonly IVaultClient _client;
        private readonly VaultOptions _options;

        public ChainedVaultConfigurationSource(IConfiguration configuration, IVaultClient client, VaultOptions options)
        {
            if (client == null)
            {
                throw new ArgumentException($"{nameof(client)} must not be null", nameof(client));
            }

            _configuration = configuration;
            _client = client;
            _options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var vaultService = new VaultService(_client);
            return new ChainedVaultConfigurationProvider(_configuration, vaultService, _options);
        }
    }
}
