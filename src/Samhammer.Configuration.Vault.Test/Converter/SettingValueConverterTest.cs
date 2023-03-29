using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Samhammer.Configuration.Vault.Converter;
using Xunit;

namespace Samhammer.Configuration.Vault.Test.Converter
{
    public class SettingValueConverterTest
    {
        private const string Prefix = "VaultKey--";

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password", true)]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/my secret/Password", true)]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password test", true)]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/", false)] // not valid: slash at the end are not allowed
        [InlineData("VaultKey--", false)] // not valid: missing secret
        [InlineData("some text VaultKey--", false)] // not valid: keyword is somewhere else in text
        public void IsVaultPrefixedSettingValue_Test(string value, bool expected)
        {
            // Arrange
            var configKeys = new Dictionary<string, string> { { "Section:Setting", value } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();
            var setting = config.GetSection("Section:Setting");

            // Act
            var actual = SettingValueConverter.IsVaultPrefixedSettingValue(setting, Prefix);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void IsVaultPrefixedSettingValue_WithExceptionOnValue()
        {
            // Arrange
            var setting = Substitute.For<IConfigurationSection>();
            setting.Value.Returns(x => throw new Exception("value cant be accessed"));

            // Act
            var actual = SettingValueConverter.IsVaultPrefixedSettingValue(setting, Prefix);

            // Assert
            actual.Should().Be(false);
        }

        [Theory]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password", "kv-v2/data/myproject/myfolder/mysecret/Password")]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/my secret/Password", "kv-v2/data/myproject/myfolder/my secret/Password")]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password test", "kv-v2/data/myproject/myfolder/mysecret/Password test")]
        public void RemoveVaultPrefixFromSettingValue_TestWithValidKeys(string value, string expected)
        {
            // Arrange
            var configKeys = new Dictionary<string, string> { { "Section:Setting", value } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();
            var setting = config.GetSection("Section:Setting");

            // Act
            var actual = SettingValueConverter.RemoveVaultPrefixFromSettingValue(setting, Prefix);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("VaultKey--kv-v2/data/myproject/myfolder/mysecret/")] // not valid: slash at the end are not allowed
        [InlineData("VaultKey--")] // not valid: missing secret
        [InlineData("some text VaultKey--")] // not valid: keyword is somewhere else in text
        public void RemoveVaultPrefixFromSettingValue_TestWithInvalidKeys(string value)
        {
            // Arrange
            var configKeys = new Dictionary<string, string> { { "Section:Setting", value } };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();
            var setting = config.GetSection("Section:Setting");

            // Act
            Action action = () => SettingValueConverter.RemoveVaultPrefixFromSettingValue(setting, Prefix);

            // Assert
            action.Should().Throw<Exception>();
        }
    }
}
