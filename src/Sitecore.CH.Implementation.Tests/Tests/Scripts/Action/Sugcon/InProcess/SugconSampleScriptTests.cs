using Moq;
using Sitecore.CH.Implementation.Scripts.Action;
using Sitecore.CH.Implementation.Tests.Setup;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sitecore.CH.Implementation.Tests.Tests.Action
{
    [Collection(MainFixture.MainCollectionName)]
    public class SugconSampleScriptTests : BaseTest
    {
        private readonly Stylelabs.M.Sdk.WebClient.IWebMClient _client;
        const string AssetTypeVideoIdentifier = "M.AssetType.Video";
        const string AssetTypeDocumentIdentifier = "M.AssetType.Document";
        const string AssetTypeImageIdentifier = "M.AssetType.Image";
        const string AssetTypeGenericIdentifier = "M.AssetType.Generic";

        public SugconSampleScriptTests(MainFixture mainFixture, ITestOutputHelper output) : base(mainFixture, output)
        {
            _client = _mClientFactory.Client;
        }

        //[Fact]
        [Theory]
        [InlineData(".jpg", AssetTypeImageIdentifier)]
        [InlineData(".mpeg", AssetTypeVideoIdentifier)]
        [InlineData(".pdf", AssetTypeDocumentIdentifier)]
        [InlineData(".xls", AssetTypeGenericIdentifier)]
        public async Task TestFileAssetTypeAutomation(string fileExtension, string expectedFileTypeidentifier)
        {
            //Arrange

            var mockContext = new Mock<IActionScriptContext>();

            //toDo create dummy asset from Service CreateDummy[Entity] methods
            var asset = await _client.EntityFactory.CreateAsync(Base.Constants.EntityDefinitions.Asset.DefinitionName).ConfigureAwait(false);
            asset.SetPropertyValue(Base.Constants.EntityDefinitions.Asset.Properties.Title, $"{Guid.NewGuid()} Asset");
            asset.SetPropertyValue(Base.Constants.EntityDefinitions.Asset.Properties.FileName, $"{Guid.NewGuid()}-Asset{fileExtension}");

            mockContext.Setup(x => x.Target).Returns(asset);
            mockContext.Setup(x => x.TargetId).Returns(asset.Id);
            mockContext.Setup(x => x.ExecutionPhase).Returns(ExecutionPhase.Pre);

            var script = new SugconSampleScript(mockContext.Object, _client);

            //Act

            await script.Run().ConfigureAwait(false);


            //Assert

            //Assert if Relation has been set as expected.

            var assetTypeToAssetRelation = await asset.GetRelationAsync(Base.Constants.EntityDefinitions.Asset.Relations.AssetTypeToAsset).ConfigureAwait(false);
            var assetTypeId = assetTypeToAssetRelation.GetIds().FirstOrDefault<long>();

            var assetTypeEntity = await _mClientFactory.Client.Entities.GetAsync(assetTypeId).ConfigureAwait(false);

            Assert.Equal(assetTypeEntity.Identifier, expectedFileTypeidentifier);

        }
    }
}
