using Moq;
using Newtonsoft.Json.Linq;
using Sitecore.CH.Implementation.Scripts.Samples.Action;
using Sitecore.CH.Implementation.Tests.Helper;
using Sitecore.CH.Implementation.Tests.Setup;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sitecore.CH.Implementation.Tests.Tests.Action
{
    [Collection(MainFixture.MainCollectionName)]
    public class SampleScriptTests : BaseTest
    {
        private readonly Stylelabs.M.Sdk.WebClient.IWebMClient _client;

        public SampleScriptTests(MainFixture mainFixture, ITestOutputHelper output) : base(mainFixture, output)
        {
            _client = _mClientFactory.Client;
        }

        [Fact]
        public async Task TitleGetsSetAsExpected()
        {
            //Arrange

            var expectedTitleValue = "SampleScriptValue";
            var mockContext = new Mock<IActionScriptContext>();
            var asset = await _client.EntityFactory.CreateAsync(Base.Constants.EntityDefinitions.Asset.DefinitionName).ConfigureAwait(false);


            mockContext.Setup(x => x.Target).Returns(asset);
            mockContext.Setup(x => x.TargetId).Returns(asset.Id);
            mockContext.Setup(x => x.ExecutionPhase).Returns(ExecutionPhase.Pre);

            var script = new SampleScript(mockContext.Object, _client);

            //Act

            await script.Run().ConfigureAwait(false);


            //Assert
            var value = asset.GetPropertyValue<string>(Base.Constants.EntityDefinitions.Asset.Properties.Title);
            Assert.NotNull(value);
            Assert.Equal(value, expectedTitleValue);

        }
    }
}
