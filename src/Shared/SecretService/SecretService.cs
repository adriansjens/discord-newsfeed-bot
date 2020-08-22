using System;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.Azure.Services.AppAuthentication;

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
            var client = new KeyClient(new Uri(_vaultUrl), new DefaultAzureCredential());

            return client.VaultUri == null ? null : client.GetKey(keyName).ToString();
        }

    }
}
