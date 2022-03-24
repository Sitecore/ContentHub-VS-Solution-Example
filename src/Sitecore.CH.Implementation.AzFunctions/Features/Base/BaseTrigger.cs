using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using System.Web.Http;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.Base.Domain;

namespace Sitecore.CH.Implementation.AzFunctions.Features.Base
{
    public abstract class BaseTrigger
    {
        protected readonly IMClientFactory _mClientFactory;
        protected readonly ILogger _logger;
        protected readonly ILoggingContextService _loggingContextService;

        public BaseTrigger(IMClientFactory mClientFactory, ILogger logger, ILoggingContextService loggingContextService)
        {
            this._mClientFactory = mClientFactory;
            this._logger = logger;
            this._loggingContextService = loggingContextService;
        }


        public async Task<IActionResult> Run(HttpRequest req)
        {
            if (req.Method == HttpMethod.Head.ToString())
                return new OkResult();

            try
            {
                var targetId = await GetRequestTargetId(req).ConfigureAwait(false);
                if (!targetId.HasValue)
                {
                    _logger.LogError(Constants.Logging.TargetIdMissing);
                    return new BadRequestResult();
                }
                var entity = await _mClientFactory.Client.Entities.GetAsync(targetId.Value, GetEntityLoadConfiguration()).ConfigureAwait(false);

                if (entity == null)
                {
                    _logger.LogError(Constants.Logging.EntityDoesNotExist(targetId.Value));
                    return new NoContentResult();
                }

                SetContext(entity);

                if (!IsValid(entity))
                {
                    _logger.LogWarning(Constants.Logging.EntityIsNotValid);
                    return new NoContentResult();
                }
                return await Execute(req, entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " " + ex.StackTrace);
                return new InternalServerErrorResult();
            }
        }

        private void SetContext(IEntity entity)
        {
            _loggingContextService.SetContext($"{entity.DefinitionName}|{entity.Id}");
        }

        protected abstract bool IsValid(IEntity entity);

        protected abstract Task<IActionResult> Execute(HttpRequest req, IEntity entity);

        protected abstract IEntityLoadConfiguration GetEntityLoadConfiguration();

        private async Task<long?> GetRequestTargetId(HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

            if (requestBody == null)
            {
                _logger.LogWarning(Constants.Logging.RequestBodyIsEmpty);
                return null;
            }
            var requestBodyObject = JsonConvert.DeserializeObject<SaveEntityMessage>(requestBody);
            if (requestBodyObject == null)
            {
                _logger.LogWarning(Constants.Logging.RequestBodyNotSerializable(requestBody));
                return null;
            }
            var targetId = requestBodyObject.SaveEntityMessageBody.TargetId;
            return targetId;
        }
    }
}
