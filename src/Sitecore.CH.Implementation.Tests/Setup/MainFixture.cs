using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Sitecore.CH.Base.Features.Base.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Sdk.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sitecore.CH.Implementation.Tests.Setup
{
    public class MainFixture : IDisposable
    {
        public const string MainCollectionName = "MainCollection";
        public Application Application;
        IWebMClient client;
        IBaseEntityService entityService;

        public MainFixture()
        {
            Application = new Application();
            Application.Startup();

            client = Base.CommandLine.Application.ServiceProvider.GetService<IMClientFactory>().Client;
            entityService = Base.CommandLine.Application.ServiceProvider.GetService<IBaseEntityService>();
        }

        private async Task EnsureRoleTemplateEntitiesExists(string identifierProperty, string identifierValue)
        {
            var entityId = await entityService.GetEntityIdAsync(Base.Constants.EntityDefinitions.RoleTemplate.DefinitionName, identifierProperty, identifierValue).ConfigureAwait(false);

            if (!entityId.HasValue)
            {
                var entity = await client.EntityFactory.CreateAsync(Base.Constants.EntityDefinitions.RoleTemplate.DefinitionName).ConfigureAwait(false);
                entity.Identifier = $"{Base.Constants.EntityDefinitions.RoleTemplate.DefinitionName}.{identifierValue}";
                entity.SetPropertyValue(identifierProperty, identifierValue);
                entity.SetPropertyValue(Base.Constants.EntityDefinitions.RoleTemplate.Properties.Template, JToken.Parse("[]"));
                await client.Entities.SaveAsync(entity).ConfigureAwait(false);
            }
        } 

        private async Task EnsureUserEntityExists(string identifierProperty, string identifierValue, List<string> userGroupNames)
        {
            var entityId = await entityService.GetEntityIdAsync(Base.Constants.EntityDefinitions.User.DefinitionName, identifierProperty, identifierValue).ConfigureAwait(false);

            if (!entityId.HasValue)
            {
                var entity = await client.EntityFactory.CreateAsync(Base.Constants.EntityDefinitions.User.DefinitionName).ConfigureAwait(false);
                entity.Identifier = $"{Base.Constants.EntityDefinitions.User.DefinitionName}.{identifierValue}";
                entity.SetPropertyValue(identifierProperty, identifierValue);

                //Set usergroups to this user
                var UserGroupToUserRelation = entity.GetRelation(Base.Constants.EntityDefinitions.User.Relations.UserGroupToUser);
                if (UserGroupToUserRelation != null)
                {
                    var UserGroupIds = await entityService.GetEntityIdsAsync(Base.Constants.EntityDefinitions.UserGroup.DefinitionName,
                        Base.Constants.EntityDefinitions.UserGroup.Properties.GroupName, userGroupNames).ConfigureAwait(false);
                    if (UserGroupIds.Any())
                    {
                        UserGroupToUserRelation.SetIds(UserGroupIds);
                    }
                }
                await client.Entities.SaveAsync(entity).ConfigureAwait(false);
            }
        }

        private async Task EnsureUserGroupEntityExists(string identifierProperty, string identifierValue)
        {
            var entityId = await entityService.GetEntityIdAsync(Base.Constants.EntityDefinitions.UserGroup.DefinitionName, identifierProperty, identifierValue).ConfigureAwait(false);

            if (!entityId.HasValue)
            {
                var entity = await client.EntityFactory.CreateAsync(Base.Constants.EntityDefinitions.UserGroup.DefinitionName).ConfigureAwait(false);
                entity.Identifier = $"{Base.Constants.EntityDefinitions.UserGroup.DefinitionName}.{identifierValue}";
                entity.SetPropertyValue(identifierProperty, identifierValue);
                await client.Entities.SaveAsync(entity).ConfigureAwait(false);
            }
        }

        private async Task EnsureSimpleEntityExists(string definitionName, string identifierProperty, string identifierValue)
        {
            var entityId = await entityService.GetEntityIdAsync(definitionName, identifierProperty, identifierValue).ConfigureAwait(false);

            if (!entityId.HasValue)
            {
                var entity = await client.EntityFactory.CreateAsync(definitionName).ConfigureAwait(false);
                entity.Identifier = $"{definitionName}.{identifierValue}";
                entity.SetPropertyValue(identifierProperty, identifierValue);
                await client.Entities.SaveAsync(entity).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
        }
    }


    [CollectionDefinition(MainFixture.MainCollectionName)]
    public class MainCollection : ICollectionFixture<MainFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
