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

        [FunctionName("SyncNewsfeed")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            try
            {
                var botService = new Sync.Sync(_services);

                await botService.SyncData();

                log.LogInformation($"ManUtd newsfeed sync function executed at: {DateTime.Now}");
            }

            catch (Exception e)
            {
                log.LogInformation(e.InnerException?.Message, e.StackTrace);
            }
        }
    }
}