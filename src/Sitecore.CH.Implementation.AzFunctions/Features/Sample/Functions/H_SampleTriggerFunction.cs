using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Sitecore.CH.Base.Features.Sample.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Implementation.AzFunctions.Features.Base;
using Sitecore.CH.Implementation.Features.Sample.Services;
using Sitecore.CH.Implementation.AzFunctions.Sample.Services;

namespace Sitecore.CH.Implementation.AzFunctions.Sample.Functions
{
    public class H_SampleTriggerFunction : BaseTrigger
    {
        private readonly ISampleImplementationService _sampleImplementationService;
        private readonly ISampleBaseService _sampleBaseService;
        private readonly ISampleAzFunctionService _sampleAzFunctionService;
        public const string FunctionName = "H_SampleTriggerFunction";

        public H_SampleTriggerFunction(IMClientFactory mClientFactory,
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
        public Task<IActionResult> EntryPoint(
            [HttpTrigger(AuthorizationLevel.Function, "head", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return base.Run(req);
        }

        protected override async Task<IActionResult> Execute(HttpRequest req, IEntity entity)
        {

            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = await client.Users.GetUserIdAsync(sampleUserNameToQuery).ConfigureAwait(false);
            _logger.LogInformation($"Test Connection {nameof(H_SampleTriggerFunction)} - {sampleUserNameToQuery} -id:{id}");

            await _sampleAzFunctionService.RunAsync().ConfigureAwait(false);
            await _sampleImplementationService.RunAsync().ConfigureAwait(false);
            await _sampleBaseService.RunAsync().ConfigureAwait(false);

            return new OkResult();
        }

        protected override IEntityLoadConfiguration GetEntityLoadConfiguration()
        {
            return EntityLoadConfiguration.Default;
        }

        protected override bool IsValid(IEntity entity)
        {
            return true;
            //return entity.DefinitionName == "M.Asset";
        }
    }
}
