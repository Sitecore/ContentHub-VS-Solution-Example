using Microsoft.Extensions.Logging;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.Features.Security.Services
{
    public interface ISecurityService
    {
        Task<bool> OnSecurityCheck(string username, long entityId, string permission);
    }

    class SecurityService : ISecurityService
    {
        private readonly ILoggerService<SecurityService> _logger;
        private readonly IMClientFactory _mClientFactory;

        public SecurityService(ILoggerService<SecurityService> logger, IMClientFactory mClientFactory)
        {
            this._logger = logger;
            this._mClientFactory = mClientFactory;
        }


        public async Task<bool> OnSecurityCheck(string username, long entityId, string permission)
        {
            var client = _mClientFactory.Client;
            var imporsonateClient = await client.ImpersonateAsync(username).ConfigureAwait(false);

            var entity = await imporsonateClient.Entities.GetAsync(entityId).ConfigureAwait(false);

            if (entity == null)
            {
                _logger.LogWarning($"OnSecurityCheck - User: {username} does not have READ access to entity {entityId} or entity does not exist)");
                return false;
            }

            var permissions = await entity.GetPermissionsAsync().ConfigureAwait(false);

            if (!permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError($"OnSecurityCheck - Insufficient Permissions ({permission}) for user: {username} on Entity: {entityId}({entity.DefinitionName})");
                return false;
            }

            return true;
        }
    }
}



