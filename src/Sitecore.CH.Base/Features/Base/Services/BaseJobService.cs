using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Sdk.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.Features.Base.Services
{
    public interface IBaseJobService
    {
        Task<long?> CreateWebFetchJob(long assetId, string fileUrl);
    }
    public class BaseJobService : IBaseJobService
    {
        private readonly ILoggerService<BaseJobService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public BaseJobService(ILoggerService<BaseJobService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }

        public async Task<long?> CreateWebFetchJob(long assetId, string fileUrl)
        {
            try
            {
                var webFetchJob = new WebFetchJobRequest($"Web fetch job for AssetId {assetId}", assetId)
                {
                    Urls = new[] { new Uri(fileUrl) }
                };

                return await _mClientFactory.Client.Jobs.CreateFetchJobAsync(webFetchJob).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " " + ex.StackTrace);
            }
            return null;
        }
    }
}
