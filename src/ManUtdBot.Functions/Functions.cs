using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ManUtdBot.Functions
{
    public class ManUtdBot
    {
        private readonly IServiceProvider _services;

        public ManUtdBot(IServiceProvider services)
        {
            _services = services;
        }

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var botService = new BotService.BotService(_services);

                await botService.Sync();

                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception e)
            {
                
            }
        }
    }
}