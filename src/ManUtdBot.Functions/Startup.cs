using System;
using System.Net.Http;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ManUtdBot.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
                .AddUserSecrets(Assembly.Load("Shared"), true)
                .Build();

            var environment = configuration["ENVIRONMENT"];

            builder.Services.AddHttpClient("TwitterApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(configuration["TwitterApiBaseAddress"]);
            }).AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddSingleton<ITwitterApiService, TwitterApiService>(provider => new TwitterApiService(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient("TwitterApiHttpClient"),
                provider.GetRequiredService<ICacheService>()));

            builder.Services.AddSingleton<ISecretService, SecretService>(provider =>
            {
                var azureKeyVaultUrl = configuration["AzureKeyVault"];
                TokenCredential credentials = new DefaultAzureCredential();
                if (environment == "Development")
                {
                    credentials = new ClientSecretCredential(
                        configuration["AZURE_TENANT_ID"],
                        configuration["AZURE_CLIENT_ID"],
                        configuration["AZURE_CLIENT_SECRET"]);
                }
                return new SecretService(new SecretClient(new Uri(azureKeyVaultUrl),
                     credentials));
            });

            var redisConnectionString = configuration["RedisConnectionString"];
            builder.Services.AddSingleton<ICacheService, CacheService>(provider =>
                new CacheService(redisConnectionString));

            builder.Services.AddMemoryCache();
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
