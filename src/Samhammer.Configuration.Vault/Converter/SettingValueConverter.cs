using System;
using Microsoft.Extensions.Configuration;

namespace Samhammer.Configuration.Vault.Converter
{
    public static class SettingValueConverter
    {
        public static bool IsVaultPrefixedSettingValue(IConfigurationSection setting, string prefix)
        {
            return setting.Value?.Length > prefix.Length
            && !setting.Value.EndsWith("/")
            && setting.Value.StartsWith(prefix);
        }

        public static string RemoveVaultPrefixFromSettingValue(IConfigurationSection setting, string prefix)
        {
            if (!IsVaultPrefixedSettingValue(setting, prefix))
            {
                throw new Exception($"The setting {setting.Key} does not contain a vault key prefixed with {prefix}");
            }

            return setting.Value.Substring(prefix.Length);
        }
    }
}
