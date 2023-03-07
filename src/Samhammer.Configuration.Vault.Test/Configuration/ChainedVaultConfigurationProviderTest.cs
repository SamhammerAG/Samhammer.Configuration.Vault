using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Samhammer.Configuration.Vault.Configuration;
using Samhammer.Configuration.Vault.Services;
using Xunit;

namespace Samhammer.Configuration.Vault.Test.Configuration
{
    public class ChainedVaultConfigurationProviderTest
    {
        private readonly IVaultService _vaultService;
        private readonly VaultOptions _options;

        public ChainedVaultConfigurationProviderTest()
        {
            _vaultService = Substitute.For<IVaultService>();
            _options = new VaultOptions
            {
                VaultKeyPrefix = "VaultKey--",
                OmitMissingSecrets = true,
            };
        }

        [Fact]
        public async Task CreateVaultKeySetting_Test()
        {
            // Arrange
            var configKeys = new Dictionary<string, string>
            {
                { "RootKeyOne", "SomeValue" },
                { "RootKeyTwo", "VaultKey--kv-v2/data/myproject/myfolder/mysecret/Username" },
                { "RootSection:SubKeyOne", "SubkeyValue" },
                { "RootSection:SubKeyTwo", "VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password" },
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();

            var provider = new ChainedVaultConfigurationProvider(config, _vaultService, _options);

            // Act
            await provider.CreateVaultKeySetting(config.GetChildren());

            // Assert
            provider.TryGet("RootKeyTwo", out var settingValue);
            settingValue.Should().BeEmpty();

            provider.TryGet("VaultKey--RootKeyTwo", out var vaultSettingValue);
            vaultSettingValue.Should().Be("kv-v2/data/myproject/myfolder/mysecret/Username");

            provider.TryGet("RootSection:SubKeyTwo", out settingValue);
            settingValue.Should().BeEmpty();

            provider.TryGet("RootSection:VaultKey--SubKeyTwo", out vaultSettingValue);
            vaultSettingValue.Should().Be("kv-v2/data/myproject/myfolder/mysecret/Password");
        }

        [Fact]
        public async Task SetVaultValues_Test()
        {
            // Arrange
            _vaultService.GetValue("kv-v2/data/myproject/myfolder/mysecret/Username", string.Empty).Returns("MyUsername");
            _vaultService.GetValue("kv-v2/data/myproject/myfolder/mysecret/Password", string.Empty).Returns("MyPassword");

            var configKeys = new Dictionary<string, string>
            {
                { "RootKeyOne", "SomeValue" },
                { "RootKeyTwo", "VaultKey--kv-v2/data/myproject/myfolder/mysecret/Username" },
                { "RootSection:SubKeyOne", "SubkeyValue" },
                { "RootSection:SubKeyTwo", "VaultKey--kv-v2/data/myproject/myfolder/mysecret/Password" },
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configKeys).Build();

            var provider = new ChainedVaultConfigurationProvider(config, _vaultService, _options);
            await provider.CreateVaultKeySetting(config.GetChildren());

            // Act
            await provider.SetVaultValues();

            // Assert
            provider.TryGet("RootKeyTwo", out var rootKeyTwoValue);
            rootKeyTwoValue.Should().Be("MyUsername");

            provider.TryGet("RootSection:SubKeyTwo", out var subKeyTwoValue);
            subKeyTwoValue.Should().Be("MyPassword");
        }
    }
}
