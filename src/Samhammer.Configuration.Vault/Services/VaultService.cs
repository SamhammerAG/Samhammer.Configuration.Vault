using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.Commons;

namespace Samhammer.Configuration.Vault.Services
{
    internal class VaultService : IVaultService
    {
        private readonly IVaultClient _client;

        public VaultService(IVaultClient client)
        {
            _client = client;
        }

        public async Task<string> GetValue(string key, string fallbackValue)
        {
            var shortKey = key.Replace("kv-v2/data/", string.Empty);
            var secretPath = shortKey.Remove(shortKey.LastIndexOf('/'));
            var secretKeyName = shortKey.Split('/').Last();

            SecretData secretData = await GetSecret(secretPath);

            if (secretData == null)
            {
                if (fallbackValue != null)
                {
                    return fallbackValue;
                }

                throw new Exception($"Secret '{secretPath}' not found");
            }

            var isExistingKey = secretData.Data.TryGetValue(secretKeyName, out var keyValue);

            if (!isExistingKey)
            {
                if (fallbackValue != null)
                {
                    return fallbackValue;
                }

                throw new Exception($"Secret '{secretPath}' does not contain a key '{secretKeyName}'");
            }

            return keyValue.ToString();
        }

        private async Task<SecretData> GetSecret(string secretPath)
        {
            try
            {
                Secret<SecretData> existingSecret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);
                return existingSecret.Data;
            }
            catch (VaultApiException e)
            {
                // Secret not existing
                if (e.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }
    }

    public interface IVaultService
    {
        Task<string> GetValue(string key, string fallbackValue);
    }
}
