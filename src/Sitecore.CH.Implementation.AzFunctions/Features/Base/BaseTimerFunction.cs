using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.AzFunctions.Features.Base
{
    public abstract class BaseTimerFunction
    {
        protected readonly IMClientFactory _mClientFactory;
        protected readonly ILogger _logger;
        protected readonly ILoggingContextService _loggingContextService;

        public BaseTimerFunction(IMClientFactory mClientFactory, ILogger logger, ILoggingContextService loggingContextService)
        {
            this._mClientFactory = mClientFactory;
            this._logger = logger;
            this._loggingContextService = loggingContextService;
        }


        public async Task Run()
        {
            try
            {

                await Execute().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        protected virtual void SetContext(IEntity entity)
        {
            _loggingContextService.SetContext($"{entity.DefinitionName}|{entity.Id}");
        }

        protected abstract Task Execute();
    }
}
