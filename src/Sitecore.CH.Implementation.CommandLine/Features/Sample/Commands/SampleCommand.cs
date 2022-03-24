using ManyConsole;
using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.CommandLine.Commands.Features.Sample.Services;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.Sample.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Sitecore.CH.Implementation.CommandLine.Features.Sample.Services;
using Sitecore.CH.Implementation.Features.Sample.Services;

namespace Sitecore.CH.Implementation.CommandLine.Features.Sample.Commands
{
    public class SampleCommand : ConsoleCommand
    {
        private const int Success = 0;
        private const int Failure = 2;
        private readonly IMClientFactory _mClientFactory;
        private readonly ILoggerService<SampleCommand> _logger;
        private readonly ISampleImplementationService _sampleImplementationService;
        private readonly ISampleBaseService _sampleBaseService;
        private readonly ISampleBaseCommandLineService _sampleBaseCommandLineService;
        private readonly ISampleCommandLineService _sampleCommandLineService;

        public SampleCommand(IMClientFactory mClientFactory,
                             ILoggerService<SampleCommand> logger,
                             ISampleImplementationService sampleImplementationService,
                             ISampleBaseService sampleBaseService,
                             ISampleBaseCommandLineService sampleBaseCommandLineService,
                             ISampleCommandLineService sampleCommandLineService
            )
        {
            IsCommand("SampleCommand", "Tests connection");
            this._mClientFactory = mClientFactory;
            this._logger = logger;
            this._sampleImplementationService = sampleImplementationService;
            this._sampleBaseService = sampleBaseService;
            this._sampleBaseCommandLineService = sampleBaseCommandLineService;
            this._sampleCommandLineService = sampleCommandLineService;
        }

        public override int Run(string[] remainingArguments)
        {
            var client = _mClientFactory.Client;
            var sampleUserNameToQuery = "superuser";
            var id = client.Users.GetUserIdAsync(sampleUserNameToQuery).Result;
            _logger.LogInformation($"Test Connection {nameof(SampleCommand)} - {sampleUserNameToQuery} - id:{id}");

            _sampleCommandLineService.Run();
            _sampleBaseCommandLineService.Run();
            _sampleImplementationService.RunAsync().Wait();
            _sampleBaseService.RunAsync().Wait();

            return Success;
        }
    }
}
