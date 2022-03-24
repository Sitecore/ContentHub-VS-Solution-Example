using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.Features.Sample.Services
{
    public interface ISampleImplementationService
    {
        Task RunAsync();
    }

    public class SampleImplementationService : ISampleImplementationService
    {
        private readonly ILoggerService<SampleImplementationService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SampleImplementationService(ILoggerService<SampleImplementationService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public async Task RunAsync()
        {

            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = await client.Users.GetUserIdAsync(sampleUserNameToQuery).ConfigureAwait(false);
            _logger.LogInformation($"Test Connection {nameof(SampleImplementationService)} - {sampleUserNameToQuery} - id:{id}");
        }

    }
}
