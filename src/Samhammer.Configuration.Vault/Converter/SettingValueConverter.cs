using System;
using Microsoft.Extensions.Configuration;

namespace Samhammer.Configuration.Vault.Converter
{
    public static class SettingValueConverter
    {
        public static bool IsVaultPrefixedSettingValue(IConfigurationSection setting, string prefix)
        {
            string value;

            try
            {
                value = setting.Value;
            }
            catch
            {
                // when previous configurationSource raises an error on the value, e.g. ConfigurationSubstitutor could not replace some placeholders
                return false;
            }

            return value?.Length > prefix.Length
                   && !value.EndsWith("/")
                   && value.StartsWith(prefix);
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
