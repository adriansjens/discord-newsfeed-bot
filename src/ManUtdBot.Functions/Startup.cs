using System;
using System.Net.Http;
using ManUtdBot.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using Shared.SecretService;

[assembly: FunctionsStartup(typeof(Startup))]


namespace ManUtdBot.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var httpClient = new HttpClient();
            builder.Services.AddSingleton(httpClient);

            var azureKeyVaultUrl = Environment.GetEnvironmentVariable("AzureKeyVault");
            var secretService = new SecretService(azureKeyVaultUrl);
            builder.Services.AddSingleton<ISecretService>(secretService);
        }
    }
}
