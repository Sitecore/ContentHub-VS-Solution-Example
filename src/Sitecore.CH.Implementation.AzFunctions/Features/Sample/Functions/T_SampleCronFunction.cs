using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.Sample.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Implementation.AzFunctions.Features.Base;
using Sitecore.CH.Implementation.AzFunctions.Sample.Services;
using Sitecore.CH.Implementation.Features.Sample.Services;
using System;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.AzFunctions.Sample.Functions
{
    public class T_SampleCronFunction : BaseTimerFunction
    {
        private readonly ISampleImplementationService _sampleImplementationService;
        private readonly ISampleBaseService _sampleBaseService;
        private readonly ISampleAzFunctionService _sampleAzFunctionService;
        public const string FunctionName = "T_SampleCronFunction";
        public T_SampleCronFunction(IMClientFactory mClientFactory,
                                       ILoggerService<H_SampleTriggerFunction> logger,
                                       ILoggingContextService loggingContextService,
                                       ISampleImplementationService sampleImplementationService,
                                       ISampleBaseService sampleBaseService,
                                       ISampleAzFunctionService sampleAzFunctionService
            ) : base(mClientFactory, logger, loggingContextService)
        {
            this._sampleImplementationService = sampleImplementationService;
            this._sampleBaseService = sampleBaseService;
            this._sampleAzFunctionService = sampleAzFunctionService;
        }
        //[FunctionName(FunctionName)] - commented so that this doesn't get pushed into a function app by mistake
        public async Task EntryPoint([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            await base.Run().ConfigureAwait(false);
        }

        protected override Task Execute()
        {
            _logger.LogInformation($"Timer function executed at {DateTime.UtcNow.ToShortDateString()}");
            return Task.CompletedTask;
        }
    }
}
