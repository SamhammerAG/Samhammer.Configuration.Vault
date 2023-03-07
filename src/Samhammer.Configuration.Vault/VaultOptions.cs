using System;

namespace Samhammer.Configuration.Vault
{
    public class VaultOptions
    {
        public const string DefaultVaultKeyPrefix = "VaultKey--";

        public string VaultKeyPrefix { get; set; }

        public TimeSpan? ReloadInterval { get; set; }

        public bool OmitMissingSecrets { get; set; }
    }
}
