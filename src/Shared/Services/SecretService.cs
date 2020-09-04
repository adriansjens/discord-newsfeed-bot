using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Shared.Services.SecretService
{
    public interface ISecretService
    {
        string GetSecret(string secretName);
    }

    public class SecretService : ISecretService
    {
        private readonly string _vaultUrl;

        public SecretService(string vaultUrl)
        {
            _vaultUrl = vaultUrl;
        }

        public string GetSecret(string secretName)
        {
            var client = new SecretClient(new Uri(_vaultUrl), new DefaultAzureCredential());

            var secret = client.GetSecret(secretName);

            return secret?.Value.Value;
        }
    }
}
