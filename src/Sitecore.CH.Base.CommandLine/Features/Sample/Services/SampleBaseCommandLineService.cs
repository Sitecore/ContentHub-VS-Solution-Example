using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Sample.Services
{
    public interface ISampleBaseCommandLineService
    {
        void Run();
    }

    public class SampleBaseCommandLineService : ISampleBaseCommandLineService
    {
        private readonly ILoggerService<SampleBaseCommandLineService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SampleBaseCommandLineService(ILoggerService<SampleBaseCommandLineService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public void Run()
        {
            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = client.Users.GetUserIdAsync(sampleUserNameToQuery).Result;
            _logger.LogInformation($"Test Connection {nameof(SampleBaseCommandLineService)} - {sampleUserNameToQuery} - id:{id}");
        }

    }
}
