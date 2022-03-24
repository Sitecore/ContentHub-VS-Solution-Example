using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.AzFunctions.Sample.Services
{
    public interface ISampleAzFunctionService
    {
        Task RunAsync();
    }

    public class SampleAzFunctionService : ISampleAzFunctionService
    {
        private readonly ILoggerService<SampleAzFunctionService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SampleAzFunctionService(ILoggerService<SampleAzFunctionService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public async Task RunAsync()
        {
            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = await client.Users.GetUserIdAsync(sampleUserNameToQuery).ConfigureAwait(false);
            _logger.LogInformation($"Test Connection {nameof(SampleAzFunctionService)} - {sampleUserNameToQuery} - id:{id}");
        }

    }
}
