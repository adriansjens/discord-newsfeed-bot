using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Shared.SecretService
{
    public interface ISecretService
    {
        string GetSecret(string keyName);
    }

    public class SecretService : ISecretService
    {
        private readonly string _vaultUrl;

        public SecretService(string vaultUrl)
        {
            _vaultUrl = vaultUrl;
        }

        public string GetSecret(string keyName)
        {
            var client = new SecretClient(new Uri(_vaultUrl), new DefaultAzureCredential());

            return client.VaultUri == null ? null : client.GetSecret(keyName).Value.Value;
        }

    }
}
