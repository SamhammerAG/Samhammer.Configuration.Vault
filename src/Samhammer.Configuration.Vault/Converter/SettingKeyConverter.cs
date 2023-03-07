using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Samhammer.Configuration.Vault.Converter
{
    public static class SettingKeyConverter
    {
        public static bool IsVaultPrefixedSettingKey(string absoluteSettingName, string prefix)
        {
            var settingName = GetSettingName(absoluteSettingName);

            return settingName?.Length > prefix.Length
                && settingName.StartsWith(prefix);
        }

        public static string AddVaultPrefixToSettingKey(IConfigurationSection valueSetting, string prefix)
        {
            if (valueSetting.Key == string.Empty)
            {
                throw new Exception($"The setting key may not be empty beneath {valueSetting.Path}");
            }

            if (valueSetting.Key.StartsWith(prefix))
            {
                throw new Exception($"The setting {valueSetting.Path} is already prefixed with {prefix}");
            }

            var parents = GetParents(valueSetting.Path);

            return string.IsNullOrEmpty(parents)
                ? $"{prefix}{valueSetting.Key}"
                : $"{parents}{ConfigurationPath.KeyDelimiter}{prefix}{valueSetting.Key}";
        }

        public static string RemoveVaultPrefixFromSettingKey(string absoluteVaultKeySettingName, string prefix)
        {
            if (!IsVaultPrefixedSettingKey(absoluteVaultKeySettingName, prefix))
            {
                throw new Exception($"The setting {absoluteVaultKeySettingName} is not a vault key setting prefixed with {prefix}");
            }

            var parents = GetParents(absoluteVaultKeySettingName);
            var settingKey = GetSettingName(absoluteVaultKeySettingName);

            var settingsKeyToUpdate = settingKey.Substring(prefix.Length);

            return string.IsNullOrEmpty(parents)
                ? settingsKeyToUpdate
                : $"{parents}{ConfigurationPath.KeyDelimiter}{settingsKeyToUpdate}";
        }

        private static string GetParents(string absoluteSetting)
        {
            var lastDelimiterIndex = absoluteSetting.LastIndexOf(ConfigurationPath.KeyDelimiter, StringComparison.OrdinalIgnoreCase);

            return lastDelimiterIndex == -1
                ? string.Empty
                : absoluteSetting.Remove(lastDelimiterIndex);
        }

        private static string GetSettingName(string absoluteSetting)
        {
            return absoluteSetting?
                .Split(new[] { ConfigurationPath.KeyDelimiter }, StringSplitOptions.None)
                .Last();
        }
    }
}
