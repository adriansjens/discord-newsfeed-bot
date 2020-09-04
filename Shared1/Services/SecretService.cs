using Azure.Security.KeyVault.Secrets;

namespace Shared.Services
{
    public interface ISecretService
    {
        string GetSecret(string secretName);
    }

    public class SecretService : ISecretService
    {
        private readonly SecretClient _client;

        public SecretService(SecretClient client)
        {
            _client = client;
        }

        public string GetSecret(string secretName)
        {

            var secret = _client.GetSecret(secretName);

            return secret?.Value.Value;
        }
    }
}
