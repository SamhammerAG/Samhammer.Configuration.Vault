using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Samhammer.Configuration.Vault.Converter;
using Samhammer.Configuration.Vault.Services;

namespace Samhammer.Configuration.Vault.Configuration
{
    // This class is inspired from Microsoft.Extensions.Configuration.ChainedConfigurationProvider
    public class ChainedVaultConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly CancellationTokenSource _cancellationToken;

        private readonly IConfiguration _config;
        private readonly VaultOptions _options;

        private readonly IVaultService _vaultService;
        private Task _pollingTask;

        private bool _disposed;

        public ChainedVaultConfigurationProvider(IConfiguration config, IVaultService vaultService, VaultOptions options)
        {
            if (options.ReloadInterval != null && options.ReloadInterval.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(options.ReloadInterval), options.ReloadInterval, $"{nameof(options.ReloadInterval)} must be positive.");
            }

            _config = config;
            _options = options;

            _vaultService = vaultService;
            _pollingTask = null;

            _cancellationToken = new CancellationTokenSource();
        }

        public override void Load() => LoadAsync(_config.GetChildren()).GetAwaiter().GetResult();

        private async Task PollForSecretChangesAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await WaitForReload().ConfigureAwait(false);
                try
                {
                    await LoadAsync(_config.GetChildren()).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Ignore errors during background update
                }
            }
        }

        internal virtual Task WaitForReload()
        {
            // ReSharper disable once PossibleInvalidOperationException because it is checked before calling PollForSecretChangesAsync
            return Task.Delay(_options.ReloadInterval.Value, _cancellationToken.Token);
        }

        private async Task LoadAsync(IEnumerable<IConfigurationSection> children)
        {
            // Saves the vault key to it's own setting before writing the value.
            // This way refreshes are possible.
            await CreateVaultKeySetting(children);

            // Sets the values from vault to the settings
            await SetVaultValues();

            // Start update polling if not already running
            if (_pollingTask == null && _options.ReloadInterval != null)
            {
                _pollingTask = PollForSecretChangesAsync();
            }
        }

        public async Task CreateVaultKeySetting(IEnumerable<IConfigurationSection> children)
        {
            foreach (var item in children)
            {
                if (SettingValueConverter.IsVaultPrefixedSettingValue(item, _options.VaultKeyPrefix))
                {
                    var vaultKeySetting = SettingKeyConverter.AddVaultPrefixToSettingKey(item, _options.VaultKeyPrefix);
                    var vaultKey = SettingValueConverter.RemoveVaultPrefixFromSettingValue(item, _options.VaultKeyPrefix);
                    Set(vaultKeySetting, vaultKey);

                    // Clear the value until it gets set by the SetSecretValues step
                    Set(item.Path, string.Empty);
                }

                await CreateVaultKeySetting(item.GetChildren());
            }
        }

        public async Task SetVaultValues()
        {
            var changedKeys = await GetChangedKeys();

            // Update values
            foreach (var keyToUpdate in changedKeys)
            {
                Set(keyToUpdate.Key, keyToUpdate.Value);
            }

            // Trigger refresh
            if (changedKeys.Count > 0)
            {
                OnReload();
            }
        }

        private async Task<Dictionary<string, string>> GetChangedKeys()
        {
            var keysToUpdate = new Dictionary<string, string>();

            foreach (var item in Data)
            {
                if (!SettingKeyConverter.IsVaultPrefixedSettingKey(item.Key, _options.VaultKeyPrefix))
                {
                    continue;
                }

                // Load current value
                var setting = SettingKeyConverter.RemoveVaultPrefixFromSettingKey(item.Key, _options.VaultKeyPrefix);
                TryGet(setting, out var oldValue);

                // Load new value
                var fallbackValue = _options.OmitMissingSecrets ? string.Empty : null; // null means exception for missing keys
                var newValue = await _vaultService.GetValue(item.Value, fallbackValue);

                // Add for update if value changed
                if (!string.Equals(oldValue, newValue))
                {
                    keysToUpdate.Add(setting, newValue);
                }
            }

            return keysToUpdate;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _cancellationToken.Cancel();
                    _cancellationToken.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
