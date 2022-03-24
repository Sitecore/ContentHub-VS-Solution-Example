using Microsoft.Extensions.DependencyInjection;
using Sitecore.CH.Base.Features.Base.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Logging;
using Stylelabs.M.Sdk.Models.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sitecore.CH.Implementation.Tests.Setup
{
    [Collection(MainFixture.MainCollectionName)]
    public abstract class BaseTest : IDisposable
    {
        protected readonly List<long> _entitiesToCleanup = new List<long>();
        protected readonly MainFixture mainFixture;
        protected readonly IMClientFactory _mClientFactory;
        protected readonly IBaseEntityService _entityService;
        private readonly ITestOutputHelper output;
        public BaseTest(MainFixture mainFixture)
        {
            this.mainFixture = mainFixture;
            _mClientFactory = Application.ServiceProvider.GetService<IMClientFactory>();
            _entityService = Application.ServiceProvider.GetService<IBaseEntityService>();
        }

        public BaseTest(MainFixture mainFixture, ITestOutputHelper output) : this(mainFixture)
        {
            this.output = output;

            var client = _mClientFactory.Client;
            client.Logger = new ConsoleLogger();
            client.Logger.OnLog += Logger_OnLog;

        }
        private void Logger_OnLog(object sender, LogEventArgs e)
        {
            output.WriteLine($"{e.LogLevel}|{e.Message}");
        }
        public virtual void Dispose()
        {
            var client = _mClientFactory.Client;
            client.Logger.OnLog -= Logger_OnLog;
            foreach (var id in _entitiesToCleanup.Distinct().ToList())
            {
                try
                {
                    client.Entities.DeleteAsync(id).Wait();
                    _entitiesToCleanup.Remove(id);
                }
                catch (Exception ex)
                {
                    _entitiesToCleanup.Remove(id);
                    continue;
                }
            }
        }
        public Task<List<long>> GetEntityIdsAsync(string definitionName, string identifierProperty, List<string> identifierValues)
        {
            return _entityService.GetEntityIdsAsync(definitionName, identifierProperty, identifierValues);
        }

        public Task<long?> GetEntityIdAsync(string definitionName, string identifierProperty, string identifierValue)
        {
            return _entityService.GetEntityIdAsync(definitionName, identifierProperty, identifierValue);
        }
        public async Task<long?> SaveEntityAsync(IEntity entity, bool addToCleanup = true)
        {
            var id = await _mClientFactory.Client.Entities.SaveAsync(entity).ConfigureAwait(false);
            if (addToCleanup)
            {
                _entitiesToCleanup.Add(id);
            }
            return id;
        }
    }
}
