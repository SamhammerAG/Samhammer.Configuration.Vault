using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Samhammer.Configuration.Vault.Services;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.Commons;
using Xunit;

namespace Samhammer.Configuration.Vault.Test.Services;

public class VaultServiceTest   
{
    private IVaultClient VaultClient { get; }
        
    private IVaultService VaultService { get; }

    public VaultServiceTest()
    {
        VaultClient = Substitute.For<IVaultClient>();
        VaultService = new VaultService(VaultClient);
    }

    [Fact]
    public async Task GetValue_ExistingSecret_ReturnsData()
    {
        // Arrange
        var data = new Dictionary<string, object>();
        data.Add("Password", "123");
        var expectedData = new SecretData { Data = data };
        var existingSecret = new Secret<SecretData> { Data = expectedData };

        VaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myproject/myfolder/mysecret").Returns(existingSecret);

        // Act
        var result = await VaultService.GetValue("kv-v2/data/myproject/myfolder/mysecret/Password", string.Empty);

        // Assert
        Assert.Equal("123", result);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "myproject/myfolder/notFound", "Secret 'myproject/myfolder/notFound' not found")]
    [InlineData(HttpStatusCode.Forbidden, "myproject/myfolder/forbidden", "Access denied for secret 'myproject/myfolder/forbidden'")]
    [InlineData(HttpStatusCode.BadRequest, "myproject/myfolder/badRequest", "Unexpected error when accessing secret 'myproject/myfolder/badRequest' with status code: '400'")]
    public async Task GetValue_Exception_ReturnsNull(HttpStatusCode statusCode, string path, string expectedMessage)
    {
        // Arrange
        var exception = new VaultApiException(statusCode, "exception from api");
        VaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path).ThrowsAsync(exception);

        // Act
        var exceptionResult = await Assert.ThrowsAsync<Exception>(() => VaultService.GetValue($"kv-v2/data/{path}/Password", null));

        // Assert
        Assert.Equal(expectedMessage, exceptionResult.Message);
    }
}
