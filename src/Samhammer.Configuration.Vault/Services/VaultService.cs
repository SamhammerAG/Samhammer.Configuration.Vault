using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.Commons;

namespace Samhammer.Configuration.Vault.Services
{
    public class VaultService : IVaultService
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

            var (statusCode, secretData) = await GetSecret(secretPath);

            if (secretData == null)
            {
                if (fallbackValue != null)
                {
                    return fallbackValue;
                }

                switch (statusCode)
                {
                    case (int)HttpStatusCode.NotFound:
                        throw new Exception($"Secret '{secretPath}' not found");
                    case (int)HttpStatusCode.Forbidden:
                        throw new Exception($"Does not have permission to access Secret '{secretPath}'");
                    default:
                        throw new Exception($"Unexpected error when accessing '{secretPath}' with status code: '{statusCode}'");
                }
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

        private async Task<(int StatusCode, SecretData Data)> GetSecret(string secretPath)
        {
            try
            {
                Secret<SecretData> existingSecret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);
                return ((int)HttpStatusCode.OK, existingSecret.Data);
            }
            catch (VaultApiException e)
            {
                if (!(e.StatusCode >= 200 && e.StatusCode <= 299))
                {
                    return (e.StatusCode, null);
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
