using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;

namespace Sitecore.CH.Implementation.CommandLine.Features.Sample.Services
{
    public interface ISampleCommandLineService
    {
        void Run();
    }

    public class SampleCommandLineService : ISampleCommandLineService
    {
        private readonly ILoggerService<SampleCommandLineService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SampleCommandLineService(ILoggerService<SampleCommandLineService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public void Run()
        {
            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = client.Users.GetUserIdAsync(sampleUserNameToQuery).Result;
            _logger.LogInformation($"Test Connection {nameof(SampleCommandLineService)} - {sampleUserNameToQuery} - id:{id}");
        }

    }
}
