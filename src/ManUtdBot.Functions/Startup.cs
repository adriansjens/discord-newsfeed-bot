using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ManUtdBot.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Shared.Services;
[assembly: FunctionsStartup(typeof(Startup))]


namespace ManUtdBot.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddHttpClient("TwitterApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(configuration["TwitterApiBaseAddress"]);
            }).AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddSingleton<ITwitterApiService, TwitterApiService>(provider => new TwitterApiService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("TwitterApiHttpClient"),
                provider.GetRequiredService<IDistributedCache>()));

            builder.Services.AddSingleton<ISecretService, SecretService>(provider =>
            {
                var azureKeyVaultUrl = configuration["AzureKeyVault"];
                return new SecretService(new SecretClient(new Uri(azureKeyVaultUrl), new DefaultAzureCredential()));
            });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSingleton<IDiscordService, DiscordService>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
