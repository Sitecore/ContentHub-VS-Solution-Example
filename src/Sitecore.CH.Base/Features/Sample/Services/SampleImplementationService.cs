using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.Features.Sample.Services
{
    public interface ISampleBaseService
    {
        Task RunAsync();
    }

    public class SampleBaseService : ISampleBaseService
    {
        private readonly ILoggerService<SampleBaseService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SampleBaseService(ILoggerService<SampleBaseService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public async Task RunAsync()
        {

            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = await client.Users.GetUserIdAsync(sampleUserNameToQuery).ConfigureAwait(false);
            _logger.LogInformation($"Test Connection {nameof(SampleBaseService)} - {sampleUserNameToQuery} - id:{id}");
        }

    }
}
