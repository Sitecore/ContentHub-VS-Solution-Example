using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.CH.Base.Features.Base.Domain;
using Sitecore.CH.Base.Features.Base.Extensions;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Base.Web.Api.Models;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Utilities;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Contracts.Querying.Generic;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.Features.Base.Services
{

    public interface IBaseEntityService
    {
        Task<List<IEntity>> GetAllStandardAssetsByMedia(string assetMediaIdentifier, IEntityLoadConfiguration elc = null, int take = 100, bool useScroll = false);
        Task<List<IEntity>> ExecuteQuery(Query query, IEntityLoadConfiguration entityLoadConfiguration = null, int take = 100, CancellationToken? cancellationToken = null, bool useScroll = false);
        Task<List<IEntity>> GetAllEntitiesById(List<long> entityIds, EntityLoadConfiguration elc, int batchSize = 50, int displayLogEvery = 100);

        Task<List<long>> GetEntityIdsAsync(string definitionName, string identifierProperty, List<string> identifierValues);
        Task<long?> GetEntityIdAsync(string definitionName, string identifierProperty, string identifierValue);
        Task<long?> GetEntityIdAsync(string identifier);

        Task<List<BusinessAuditLogEntry>> ExecuteBusinessAuditLogQuery(long? targetId, string eventType = null, bool useScroll = false);
        Task<bool> SetDeliverablesLifeCycleStatus(long entityId, string status, string statusRelation, string reasonProperty, string reason = "");

        /// <summary>
        /// Helper method to workaround CS0211206
        /// SDK Currently does not support self relations, so this method can help us with that
        /// </summary>
        /// <param name="childEntityId"></param>
        /// <param name="relationName"></param>
        /// <param name="cardinality"></param>
        /// <param name="parentEntityIdToAppend"></param>
        /// <returns></returns>
        Task AppendParentRelationUsingRawApiCall(long childEntityId, string relationName, Cardinality cardinality, long parentEntityIdToAppend);

        Task<long?> CopyEntity(long? id, EntityCopyOptions entityCopyOptions);
        Task<EntityRolesResource> GetRolesAsync(long id);
        Task<bool> SetUserRoleAsync(long entityId, long userId, string Role);

        Task<Link> GetEntityLinkAsync(long value);
        Task SetRolesAsync(long entityId, Dictionary<string, List<Link>> userRoles, Dictionary<string, List<Link>> userGroupRoles);
    }

    public class BaseEntityService : IBaseEntityService
    {
        private readonly IMClientFactory _mClientFactory;
        private readonly IWebMClient _client;
        private readonly ILoggerService<BaseEntityService> _logger;

        public BaseEntityService(IMClientFactory mClientFactory, ILoggerService<BaseEntityService> logger)
        {
            _mClientFactory = mClientFactory;
            _client = _mClientFactory.Client;
            _logger = logger;
        }

        public async Task<List<IEntity>> ExecuteQuery(Query query, IEntityLoadConfiguration entityLoadConfiguration = null, int take = 100, CancellationToken? cancellationToken = null, bool useScroll = false)
        {
            List<IEntity> result;
            query.Take = take;
            if (useScroll)
            {
                result = await GetEntitiesByScroll(query, entityLoadConfiguration, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                result = await GetEntitiesByQuery(query, entityLoadConfiguration, cancellationToken).ConfigureAwait(false);
            }
            return result;
        }
        public async Task<List<IEntity>> GetAllStandardAssetsByMedia(string assetMediaIdentifier, IEntityLoadConfiguration elc = null, int take = 100, bool useScroll = false)
        {
            var contentRepositoryStandard = await _client.Entities.GetAsync(Constants.Identifiers.MContentRepositoryStandard).ConfigureAwait(false);

            var assetMediaEntity = await _client.Entities.GetAsync(assetMediaIdentifier).ConfigureAwait(false);
            if (assetMediaEntity == null)
            {
                _logger.LogWarning(Constants.Logging.IdentifierDoesNotExist(assetMediaIdentifier));
                return null;
            }
            if (contentRepositoryStandard == null)
            {
                _logger.LogWarning(Constants.Logging.EntityHasNotBeenFound(Constants.Identifiers.MContentRepositoryStandard));
                return null;
            }

            var query = Query.CreateQuery(entities =>
                (from e in entities
                 where
                    e.DefinitionName == Constants.EntityDefinitions.Asset.DefinitionName &&
                    e.Parent(Constants.Relations.AssetMediaToAsset) == assetMediaEntity.Id.Value &&
                    e.Parent(Constants.Relations.ContentRepositoryToAsset) == contentRepositoryStandard.Id.Value
                 select e));
            if (query == null)
            {
                _logger.LogWarning(Constants.Logging.UnableToGetAssetMediaQuery);
                return null;
            }

            var operationName = $"{nameof(GetAllStandardAssetsByMedia)}- {assetMediaIdentifier}";
            _logger.LogInformation(Constants.Logging.BeginOperation(operationName));
            var resultAssets = await ExecuteQuery(query, elc, take, useScroll: useScroll).ConfigureAwait(false);
            _logger.LogInformation(Constants.Logging.DoneOperation(operationName));
            return resultAssets;
        }


        private async Task<List<IEntity>> GetEntitiesByScroll(Query query, IEntityLoadConfiguration entityLoadConfiguration, CancellationToken? cancellationToken = null)
        {
            var result = new List<IEntity>();
            var scroller = _client.Querying.CreateEntityScroller(query, loadConfiguration: entityLoadConfiguration);
            var itemCount = 0;
            while (await scroller.MoveNextAsync().ConfigureAwait(false))
            {
                if (!scroller.Current.Items.Any() || cancellationToken?.IsCancellationRequested == true)
                    //workaround for MONE-27343
                    break;
                itemCount = AddItems(itemCount, scroller.Current, result);
            }
            return result;
        }

        private async Task<List<IEntity>> GetEntitiesByQuery(Query query, IEntityLoadConfiguration entityLoadConfiguration, CancellationToken? cancellationToken)
        {
            var result = new List<IEntity>();
            var iterator = _client.Querying.CreateEntityIterator(query, entityLoadConfiguration);
            var itemCount = 0;
            while (await iterator.MoveNextAsync().ConfigureAwait(false))
            {
                itemCount = AddItems(itemCount, iterator.Current, result);
                if (cancellationToken?.IsCancellationRequested == true)
                    break;
            }

            return result;
        }

        private int AddItems(int itemCount, IQueryResult<IEntity> current, List<IEntity> entities, int displayLogEvery = 100)
        {
            foreach (var item in current.Items)
            {
                entities.Add(item);
                _logger.LogDebug(Constants.Logging.EntityHasBeenFound(item.Id.Value));
                itemCount++;
                if (itemCount % displayLogEvery == 0)
                    _logger.LogInformation(Constants.Logging.GotXEntitiesOf(itemCount, current.TotalNumberOfResults));
            }
            return itemCount;
        }

        public async Task<List<IEntity>> GetAllEntitiesById(List<long> entityIds, EntityLoadConfiguration elc, int batchSize = 50, int displayLogEvery = 100)
        {
            var operationName = nameof(GetAllEntitiesById);
            _logger.LogInformation(Constants.Logging.BeginOperation(operationName));
            var batches = entityIds.SplitIntoBatches(batchSize);
            var result = new List<IEntity>();
            foreach (var batch in batches)
            {
                result.AddRange(await _client.Entities.GetManyAsync(batch, elc).ConfigureAwait(false));

                if (result.Count % displayLogEvery == 0)
                    _logger.LogInformation(Constants.Logging.GotXEntitiesOf(result.Count, entityIds.Count));
            }
            _logger.LogInformation(Constants.Logging.DoneOperation(operationName));

            return result;
        }

        public async Task<List<long>> GetEntityIdsAsync(string definitionName, string identifierProperty, List<string> identifierValues)
        {
            Guard.NotNullOrEmpty(nameof(definitionName), definitionName);
            Guard.NotNullOrEmpty(nameof(identifierProperty), identifierProperty);
            Guard.NotNull(nameof(identifierValues), identifierValues);

            var client = _mClientFactory.Client;
            var query = Query.CreateIdsQuery(entities =>
                from e in entities
                where e.DefinitionName == definitionName
                && e.Property(identifierProperty).In(identifierValues)
                select e);

            var queryResult = await client.Querying.QueryIdsAsync(query).ConfigureAwait(false);

            return queryResult.Items.ToList();
        }

        public async Task<long?> GetEntityIdAsync(string definitionName, string identifierProperty, string identifierValue)
        {
            Guard.NotNullOrEmpty(nameof(definitionName), definitionName);
            Guard.NotNullOrEmpty(nameof(identifierProperty), identifierProperty);
            Guard.NotNullOrEmpty(nameof(identifierValue), identifierValue);

            var client = _mClientFactory.Client;
            var query = Query.CreateIdsQuery(entities =>
                from e in entities
                where e.DefinitionName == definitionName
                && e.Property(identifierProperty) == identifierValue
                select e);

            var result = await client.Querying.SingleIdAsync(query).ConfigureAwait(false);

            return result;
        }

        public async Task<EntityRolesResource> GetRolesAsync(long id)
        {
            var client = _mClientFactory.Client;

            var routes = await client.Api.GetApiRoutesAsync().ConfigureAwait(false);

            var all_roles_for_entity_route = routes[Constants.Api.AllRolesForEntity.TemplateName];

            var relationsLink = all_roles_for_entity_route.Bind(new Dictionary<string, string>
            {
                [Constants.Api.AllRolesForEntity.id] = id.ToString(),
            });

            HttpResponseMessage response = await client.Raw.GetAsync(relationsLink).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<EntityRolesResource>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }


        public async Task AppendParentRelationUsingRawApiCall(long childEntityId, string relationName, Cardinality cardinality, long parentEntityIdToAppend)
        {
            var client = _mClientFactory.Client;
            var routes = await client.Api.GetApiRoutesAsync().ConfigureAwait(false);

            var entity_relation_by_name = routes[Stylelabs.M.Sdk.Constants.Api.EntityRelationByName.TemplateName];

            var relationsLink = entity_relation_by_name.Bind(new Dictionary<string, string>
            {
                [Stylelabs.M.Sdk.Constants.Api.EntityRelationByName.Id] = childEntityId.ToString(),
                [Stylelabs.M.Sdk.Constants.Api.EntityRelationByName.Name] = relationName
            });

            var response = await client.Raw.GetAsync(relationsLink).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var relation = await response.Content.ReadAsJsonAsync<RelationResource>().ConfigureAwait(false);

            var entity_by_id = routes[Stylelabs.M.Sdk.Constants.Api.EntityById.TemplateName];
            var entityIdToAppendLink = entity_by_id.Bind(new Dictionary<string, string> { [Stylelabs.M.Sdk.Constants.Api.EntityById.Id] = parentEntityIdToAppend.ToString() });
            var entityIdToAppendEntityLink = new EntityLink(entityIdToAppendLink);

            switch (cardinality)
            {
                case Cardinality.OneToOne:
                case Cardinality.OneToMany:
                    relation.Parent = entityIdToAppendEntityLink;
                    break;
                case Cardinality.ManyToMany:

                    var listToSet = new List<EntityLink>();
                    if (relation.Parents != null)
                        listToSet.AddRange(relation.Parents);
                    listToSet.Add(entityIdToAppendEntityLink);
                    break;
                default:
                    throw new NotImplementedException();
            }
            var putResponse = await client.Raw.PutAsync(relationsLink, new JsonContent(relation)).ConfigureAwait(false);
            putResponse.EnsureSuccessStatusCode();
        }

        public async Task<long?> CopyEntity(long? id, EntityCopyOptions entityCopyOptions)
        {
            //Call WebAPI to copy the fragment based on entityCopyOptions
            var client = _mClientFactory.Client;

            var routes = await client.Api.GetApiRoutesAsync().ConfigureAwait(false);

            var copyEntityRoute = routes[Constants.Api.CopyEntity.TemplateName];

            var copyEntityLink = copyEntityRoute.Bind(new Dictionary<string, string>
            {
                [Constants.Api.CopyEntity.id] = id.ToString(),
            });

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(entityCopyOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

            HttpResponseMessage response = await client.Raw.PostAsync(copyEntityLink, httpContent).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return long.Parse(response.Headers.Location.Segments.LastOrDefault());
        }

        public async Task<bool> SetUserRoleAsync(long entityId, long userId, string role)
        {
            bool userAdded = false;
            var client = _mClientFactory.Client;
            var currentRoles = await GetRolesAsync(entityId).ConfigureAwait(false);

            if (!currentRoles.UserRoles.ContainsKey(role))
                currentRoles.UserRoles.Add(role, new List<Link>());

            var entityLink = await GetEntityLinkAsync(userId).ConfigureAwait(false);
            if (!currentRoles.UserRoles[role].Exists(l => l.Uri == entityLink.Uri))
            {
                currentRoles.UserRoles[role].Add(entityLink);

                await SetRolesAsync(entityId, currentRoles.UserRoles, currentRoles.UserGroupRoles).ConfigureAwait(false);

                userAdded = true;
            }
            return userAdded;

        }

        public async Task SetRolesAsync(long entityId, Dictionary<string, List<Link>> userRoles, Dictionary<string, List<Link>> userGroupRoles)
        {
            Guard.NotNull(nameof(userRoles), userRoles);
            Guard.NotNull(nameof(userGroupRoles), userGroupRoles);

            var client = _mClientFactory.Client;

            var roles = new SetRolesCommandResource
            {
                TargetId = entityId,
                UserRoles = userRoles,
                UserGroupRoles = userGroupRoles
            };

            await client.Commands.ExecuteCommandAsync(Sitecore.CH.Base.Constants.Commands.NameSpace.Security,
               Constants.Commands.Command.SetRoles, JObject.FromObject(roles)).ConfigureAwait(false);

        }

        public async Task<bool> SetDeliverablesLifeCycleStatus(long entityId, string status, string statusRelation, string reasonProperty, string reason = "")
        {
            try
            {
                var client = _mClientFactory.Client;

                var deliverablesLifeCycleStatusResource = new SetDeliverablesLifeCycleStatusResource
                {
                    EntityId = entityId,
                    Status = status,
                    Reason = reason,
                    ReasonProperty = reasonProperty,
                    StatusRelation = statusRelation,
                };

                await client.Commands.ExecuteCommandAsync(Constants.Commands.NameSpace.Project,
                   Constants.Commands.Command.SetDeliverablesLifeCycleStatus, JObject.FromObject(deliverablesLifeCycleStatusResource)).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " " + ex.StackTrace);
            }
            return false;
        }

        public async Task<Link> GetEntityLinkAsync(long value)
        {
            var client = _mClientFactory.Client;

            var routes = await client.Api.GetApiRoutesAsync().ConfigureAwait(false);

            var entityById_route = routes[Stylelabs.M.Sdk.Constants.Api.EntityById.TemplateName];

            var entityById = entityById_route.Bind(new Dictionary<string, string>
            {
                [Stylelabs.M.Sdk.Constants.Api.EntityById.Id] = value.ToString(),
            });

            return new Link(entityById);
        }

        public async Task<long?> GetEntityIdAsync(string identifier)
        {
            var client = _mClientFactory.Client;
            var entity = await client.Entities.GetAsync(identifier, EntityLoadConfiguration.Minimal).ConfigureAwait(false);
            return entity?.Id;
        }

        public async Task<List<BusinessAuditLogEntry>> ExecuteBusinessAuditLogQuery(long? targetId, string eventType = null, bool useScroll = false)
        {
            var client = _mClientFactory.Client;
            var url = string.Concat(Constants.Api.BusinessAudit.Endpoint, useScroll ? "scroll" : "query", Constants.Api.BusinessAudit.Template);
            var endPointLink = new Link(url, "businessAudit", true);

            var bindings = new Dictionary<string, string>();
            if (targetId.HasValue)
                bindings["entityId"] = targetId.ToString();
            if (!string.IsNullOrEmpty(eventType))
                bindings["raw"] = $"event_type:{eventType}";
            bindings["take"] = 1.ToString();
            if (useScroll)
                bindings["scrollTime"] = "00:00:30";

            var endpoint = endPointLink.Bind(bindings);

            var keepGoing = true;

            var result = new List<BusinessAuditLogEntry>();

            while (keepGoing)
            {
                var response = await client.Raw.GetAsync(endpoint).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var businessAuditQuery = await response.Content.ReadAsJsonAsync<BusinessAuditQuery>().ConfigureAwait(false);

                result.AddRange(businessAuditQuery.Items);
                endpoint = businessAuditQuery?.Next?.Uri;
                keepGoing = businessAuditQuery?.Items?.Any() == true && businessAuditQuery.Next != null;
            }
            return result;
        }
    }
}