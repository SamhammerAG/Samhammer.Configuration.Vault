using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Samhammer.Configuration.Vault.Converter;
using Xunit;

namespace Samhammer.Configuration.Vault.Test.Converter
{
    public class SettingKeyConverterTest
    {
        private const string Prefix = "VaultKey--";

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("MySection:VaultKey--MySetting", true)]
        [InlineData("VaultKey--MyRootSetting", true)]
        [InlineData("MySection:MySetting:VaultKey--", false)] // not valid: prefix without setting name
        [InlineData("VaultKey--", false)] // not valid: keyword without setting name
        [InlineData("VaultKey--MySection:MySetting", false)] // not valid: entire section is prefixed
        public void IsVaultPrefixedSettingKey_Test(string value, bool expected)
        {
            // Act
            var actual = SettingKeyConverter.IsVaultPrefixedSettingKey(value, Prefix);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("MySection:VaultKey--MySetting", "MySection:MySetting")]
        [InlineData("MySection:MySubSection:VaultKey--MySetting", "MySection:MySubSection:MySetting")]
        [InlineData("VaultKey--MyRootSetting", "MyRootSetting")]
        public void RemoveVaultPrefixFromSettingKey_ValidSettings(string value, string expected)
        {
            // Act
            var actual = SettingKeyConverter.RemoveVaultPrefixFromSettingKey(value, Prefix);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("MySection:MySetting:VaultKey--")] // not valid: prefix without setting name
        [InlineData("VaultKey--")] // not valid: keyword without setting name
        [InlineData("VaultKey--MySection:MySetting")] // not valid: entire section is prefixed
        public void RemoveVaultPrefixFromSettingKey_InvalidSettings(string value)
        {
            // Act
            Action action = () => SettingKeyConverter.RemoveVaultPrefixFromSettingKey(value, Prefix);

            // Assert
            action.Should().Throw<Exception>();
        }

        [Theory]
        [InlineData("MySection:MySetting", "MySection:VaultKey--MySetting")]
        [InlineData("MySection:MySubSection:MySetting", "MySection:MySubSection:VaultKey--MySetting")]
        [InlineData("MyRootSetting", "VaultKey--MyRootSetting")]
        public void AddVaultPrefixToSettingKey_ValidSettings(string value, string expected)
        {
            // Arrange
            var configKeys = new Dictionary<string, string> { { value, "SomeRandomValue" } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();
            var setting = config.GetSection(value);

            // Act
            var actual = SettingKeyConverter.AddVaultPrefixToSettingKey(setting, Prefix);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("MySection:VaultKey--MySetting")] // not valid: already prefixed
        [InlineData("MySection:MySetting:VaultKey--")] // not valid: only prefix
        [InlineData("VaultKey--MySetting")] // not valid: already prefixed root
        [InlineData("VaultKey--")] // not valid: only prefix

        public void AddVaultPrefixToSettingKey_InvalidSettings(string value)
        {
            // Arrange
            var configKeys = new Dictionary<string, string> { { value, "SomeRandomValue" } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();
            var setting = config.GetSection(value);

            // Act
            Action action = () => SettingKeyConverter.AddVaultPrefixToSettingKey(setting, Prefix);

            // Assert
            action.Should().Throw<Exception>();
        }
    }
}
