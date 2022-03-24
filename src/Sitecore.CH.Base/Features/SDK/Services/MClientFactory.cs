using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sitecore.CH.Base.Features.SDK.Services.Config;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;
using System;
using System.Linq;

namespace Sitecore.CH.Base.Features.SDK.Services
{
    public interface IMClientFactory
    {
        IWebMClient Client { get; }
    }
    public class MClientFactory : IMClientFactory
    {
        private readonly MClientOptions _options;
        private readonly ILogger<MClientFactory> _logger;
        private IWebMClient _client;
        public MClientFactory(IOptions<MClientOptions> options, ILogger<MClientFactory> logger)
        {
            this._options = options.Value;
            this._logger = logger;
        }

        public IWebMClient Client
        {
            get
            {
                if (_client == null)
                {
                    var mUri = new Uri(_options.Host);

                    var auth = new OAuthPasswordGrant()
                    {
                        ClientId = _options.ClientId,
                        ClientSecret = _options.ClientSecret,
                        UserName = _options.UserName,
                        Password = _options.Password
                    };
                    _client = Stylelabs.M.Sdk.WebClient.MClientFactory.CreateMClient(mUri, auth);
                    var knownSSoRedirects = _options.KnownSSoRedirects;
                    if (knownSSoRedirects != null && knownSSoRedirects.Any())
                        _client.SetKnownSSoRedirects(knownSSoRedirects.Select(s => new Uri(s)));

                    _logger.LogInformation("Creating {0} for environment {1}, for user {2}", nameof(IWebMClient), mUri, _options.UserName);
                }

                return _client;
            }
        }
    }
}
