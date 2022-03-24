using Moq;
using Stylelabs.M.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Implementation.Tests.Helper
{
    public class MockHelper
    {
        public static Moq.Mock<IMClient> GetMockClient(IMClient mClient)
        {
            var moq = new Mock<IMClient>(MockBehavior.Strict);
            moq.Setup(client => client.Notifications).Returns(mClient.Notifications);
            moq.Setup(client => client.Assets).Returns(mClient.Assets);
            moq.Setup(client => client.Users).Returns(mClient.Users);
            moq.Setup(client => client.Settings).Returns(mClient.Settings);
            moq.Setup(client => client.Scripts).Returns(mClient.Scripts);
            moq.Setup(client => client.Querying).Returns(mClient.Querying);
            moq.Setup(client => client.Policies).Returns(mClient.Policies);
            moq.Setup(client => client.DataSourceFactory).Returns(mClient.DataSourceFactory);
            moq.Setup(client => client.EntityFactory).Returns(mClient.EntityFactory);
            moq.Setup(client => client.EntityDefinitions).Returns(mClient.EntityDefinitions);
            moq.Setup(client => client.Entities).Returns(mClient.Entities);
            moq.Setup(client => client.DataSources).Returns(mClient.DataSources);
            moq.Setup(client => client.Cultures).Returns(mClient.Cultures);
            moq.Setup(client => client.Commands).Returns(mClient.Commands);
            moq.Setup(client => client.Logger).Returns(mClient.Logger);
            moq.Setup(client => client.Jobs).Returns(mClient.Jobs);
            return moq;
        }
    }
}
