using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Domain;
using Sitecore.CH.Base.Features.Logging.Services;
using Sitecore.CH.Base.Features.SDK.Services;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Filters;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Framework.Utilities;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Services
{
    public class ScriptService
    {
        private readonly LocalScriptDiscoveryService localScriptDiscovery;
        private readonly ILoggerService<ScriptService> Logger;
        private readonly IMClientFactory _mClientFactory;
        private readonly RelationLoadOption _relationLoadOption = new RelationLoadOption(new[] { "ScriptToActiveScriptContent", "ScriptToDraftScriptContent", "ScriptToDraftScriptContent" });
        private readonly PropertyLoadOption _propertyLoadOption = new PropertyLoadOption(new[] { "M.Script.Name", "M.Script.Type", "M.Script.Restricted", "M.Script.Enabled", "M.Script.Description", "M.Script.Name", "M.Script.HasDraft" });
        public ScriptService(LocalScriptDiscoveryService localScriptDiscovery, ILoggerService<ScriptService> logger, IMClientFactory mClientFactory)
        {
            this.localScriptDiscovery = localScriptDiscovery;
            this.Logger = logger;
            this._mClientFactory = mClientFactory;
        }
        public async Task PushScripts(string[] _scripts, bool createAction, bool? enableAssociatedTriggers = null)
        {
            if (_scripts == null || !_scripts.Any())
            {
                throw new Exception("Parameter 'scripts' is mandatory");
            }

            var updateAllScripts = _scripts.Length == 1 && _scripts[0] == "all";

            var scriptTypes = new List<ScriptCollectionType>()
            {
                new ScriptCollectionType()
                {
                    DirectoryName = "Action",
                    ScriptType = "Action",
                    CanMapToAction = true,
                    Scripts = new List<LocalScriptData>()
                },
                new ScriptCollectionType()
                {
                    DirectoryName = "WebAPI",
                    ScriptType = "Action",
                    CanMapToAction = false,
                    Scripts = new List<LocalScriptData>()
                },
                new ScriptCollectionType()
                {
                    DirectoryName = "UserSignIn",
                    ScriptType = "User_SignIn",
                    CanMapToAction = false,
                    Scripts = new List<LocalScriptData>()
                },
                new ScriptCollectionType()
                {
                    DirectoryName = "Metadata",
                    ScriptType = "Processing_Metadata",
                    CanMapToAction = false,
                    Scripts = new List<LocalScriptData>()
                }
            };

            var localDiscovery = localScriptDiscovery;

            //get scripts by type/subdirectory
            foreach (var scriptType in scriptTypes)
                scriptType.Scripts = localDiscovery.FindActionScripts(scriptType);

            foreach (var scriptType in scriptTypes)
            {
                var scripts = new List<LocalScriptData>();
                if (scriptType.Scripts == null || !scriptType.Scripts.Any())
                    continue;

                if (updateAllScripts)
                {
                    scripts = scriptType.Scripts.ToList();
                    Logger.LogWarning($"Updating and publishing all {scriptType.DirectoryName.ToUpper()}/{scriptType.ScriptType.ToUpper()} scripts.");
                }
                else
                {
                    foreach (var inputScript in _scripts)
                    {
                        //var matches = scriptType.Scripts.Where(x => Regex.IsMatch(inputScript, x.ScriptIdentifier)).ToList();
                        var matches = scriptType.Scripts.Where(x => x.ScriptIdentifier.Contains(inputScript)).ToList();
                        if (!matches.Any())
                            continue;
                        foreach (var match in matches.Where(match => !scripts.Contains(match)))
                            scripts.Add(match);
                    }
                }

                await CreateOrUpdateEnvironmentScriptsFromLocalScripts(scripts, scriptType.CanMapToAction, createAction, enableAssociatedTriggers).ConfigureAwait(false);
            }
        }

        #region environment updater

        public async Task CreateOrUpdateEnvironmentScriptsFromLocalScripts(IEnumerable<LocalScriptData> actionScripts, bool canMapToAction, bool _createAction, bool? enableAssociatedTriggers = null)
        {
            foreach (var scriptInfo in actionScripts)
            {

                try
                {
                    var scriptId = await CreateOrUpdateActionScript(scriptInfo).ConfigureAwait(false);
                    if (canMapToAction)
                    {
                        if (_createAction)
                        {
                            await CreateActionIfAbsent(scriptInfo, scriptId);
                        }
                        if (enableAssociatedTriggers.HasValue)
                        {
                            await EnableOrDisableAssociatedTriggers(scriptId, enableAssociatedTriggers.Value).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to create or update script: {scriptInfo.Name}");
                }
            }
        }

        private async Task EnableOrDisableAssociatedTriggers(long scriptId, bool enableFlagForTriggers)
        {
            var client = _mClientFactory.Client;
            const string ActionToScriptRelationName = Base.Constants.EntityDefinitions.Script.Relations.ActionToScript;

            const string TriggerToActionRelationName = Base.Constants.EntityDefinitions.Action.Relations.TriggerToAction;
            const string TriggerToPreActiontRelationName = Base.Constants.EntityDefinitions.Action.Relations.TriggerToPreAction;
            const string TriggerToSecurityPreActionRelationName = Base.Constants.EntityDefinitions.Action.Relations.TriggerToSecurityPreAction;
            const string TriggerToValidationPreActionRelationName = Base.Constants.EntityDefinitions.Action.Relations.TriggerToValidationPreAction;
            const string TriggerToAuditPreActionRelationName = Base.Constants.EntityDefinitions.Action.Relations.TriggerToAuditPreAction;

            var triggerRelationNames = new List<string>() {
                TriggerToActionRelationName,
                TriggerToPreActiontRelationName,
                TriggerToSecurityPreActionRelationName,
                TriggerToValidationPreActionRelationName,
                TriggerToAuditPreActionRelationName };

            var scriptEntity = await client.Entities.GetAsync(scriptId, new EntityLoadConfiguration(CultureLoadOption.Default, PropertyLoadOption.None, new RelationLoadOption(ActionToScriptRelationName))).ConfigureAwait(false);

            var actionToScriptRelation = scriptEntity.GetRelation(ActionToScriptRelationName, RelationRole.Child);
            var relatedActionIds = actionToScriptRelation?.GetIds();

            if (relatedActionIds?.Any() == true)
            {
                var triggerIds = new HashSet<long>();
                foreach (var actionId in relatedActionIds)
                {
                    var actionEntity = await client.Entities.GetAsync(actionId, new EntityLoadConfiguration(CultureLoadOption.Default, PropertyLoadOption.None, new RelationLoadOption(triggerRelationNames))).ConfigureAwait(false);

                    foreach (var triggerRelationName in triggerRelationNames)
                    {
                        var triggerRelation = actionEntity.GetRelation(triggerRelationName, RelationRole.Child);

                        var triggerRelationIds = triggerRelation?.GetIds();

                        if (triggerRelationIds?.Any() == true)
                        {
                            foreach (var triggerId in triggerRelationIds)
                            {
                                triggerIds.Add(triggerId);
                            }
                        }
                    }
                }

                if (triggerIds.Any())
                    await EnableOrDisableTriggers(triggerIds, enableFlagForTriggers).ConfigureAwait(false);
            }
        }

        private async Task EnableOrDisableTriggers(IEnumerable<long> triggerIds, bool enableFlagForTriggers)
        {
            var triggerBatch = triggerIds.Batch(50);
            var client = _mClientFactory.Client;
            const string IsEnabledPropertyName = Base.Constants.EntityDefinitions.Trigger.Properties.IsEnabled;
            const string NamePropertyName = Base.Constants.EntityDefinitions.Trigger.Properties.Name;
            foreach (var myTriggerIds in triggerBatch)
            {
                var triggerEntities = await client.Entities.GetManyAsync(myTriggerIds, new EntityLoadConfiguration(CultureLoadOption.Default, new PropertyLoadOption(IsEnabledPropertyName, NamePropertyName), RelationLoadOption.None)).ConfigureAwait(false);

                foreach (var triggerEntity in triggerEntities)
                {

                    triggerEntity.SetPropertyValue(IsEnabledPropertyName, enableFlagForTriggers);
                    if (triggerEntity.IsDirty)
                    {
                        var triggerName = triggerEntity.GetPropertyValue(NamePropertyName);
                        Logger.LogInformation("Trigger -\"{triggerName}\" {triggerStatus}", triggerName, enableFlagForTriggers ? "Enabled" : "Disabled");
                        await client.Entities.SaveAsync(triggerEntity).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<long> CreateOrUpdateActionScript(LocalScriptData scriptInfo)
        {
            Logger.LogDebug($"Processing script: \"{scriptInfo.Name}\"");

            long scriptId;
            var scriptEntity = await _mClientFactory.Client.Entities.GetAsync(scriptInfo.ScriptIdentifier, new EntityLoadConfiguration(CultureLoadOption.Default, _propertyLoadOption, _relationLoadOption)).ConfigureAwait(false);

            if (scriptEntity != null)
            {
                scriptId = scriptEntity.Id.Value;
                Logger.LogDebug($"Script exists, id: {scriptId}");

                await UpdateScriptMetadata(scriptEntity, scriptInfo.Name, scriptInfo.Description).ConfigureAwait(false);

                // only update changed scripts
                var hasScriptContentChanged = await HasScriptChanged(scriptEntity, scriptInfo.ScriptBody);

                // when script has unpublished changes its behavior is wierd, like new logs are not shown
                // it would be better to have it properly updated, compiled and published
                var hasUnpublishedDraft = scriptEntity.GetPropertyValue<bool>("M.Script.HasDraft");

                if (!hasScriptContentChanged && !hasUnpublishedDraft)
                {
                    Logger.LogDebug($"Script content has not changed in source file, skipping update");
                    return scriptId;
                }

                if (hasScriptContentChanged)
                    Logger.LogInformation($"Script {scriptInfo.Name} content has changed in source file, updating script in environment...");
                else
                    Logger.LogInformation($"Script {scriptInfo.Name} content is up to date but has unpublished changes, updating script in environment...");
            }
            else
            {
                Logger.LogInformation($"Script \"{scriptInfo.Name}\" does not exist. Creating new script...");
                scriptId = await CreateActionScript(scriptInfo).ConfigureAwait(false);

                Logger.LogDebug($"Creating script draft, compiling and publishing it...");
            }

            var draftId = await CreateOrUpdateDraftScript(scriptId, scriptInfo.ScriptBody).ConfigureAwait(false);
            if (draftId.HasValue)
            {
                var scriptCompiled = await CompileScript(draftId.Value).ConfigureAwait(false);
                if (scriptCompiled)
                {
                    await PublishScript(draftId.Value, scriptId).ConfigureAwait(false);
                }
            }

            return scriptId;
        }


        async Task<bool> HasScriptChanged(IEntity script, string newContent)
        {
            var scriptContent = await GetScriptActiveContent(script);
            if (string.IsNullOrEmpty(scriptContent) && !string.IsNullOrEmpty(newContent))
                return true;

            if (scriptContent != null && scriptContent.Equals(newContent))
                return false;

            return true;
        }

        async Task UpdateScriptMetadata(IEntity scriptEntity, string name, string description)
        {
            scriptEntity.SetPropertyValue("M.Script.Name", name);
            scriptEntity.SetPropertyValue("M.Script.Description", description);
            if (scriptEntity.IsDirty)
            {
                Logger.LogInformation($"Script name or description has changed, updating them");
                await _mClientFactory.Client.Entities.SaveAsync(scriptEntity).ConfigureAwait(false);
            }
            else
            {
                Logger.LogDebug($"Script name or description has not changed");
            }
        }

        async Task<string> GetScriptActiveContent(IEntity script)
        {
            var activeContentLoadConfig = new EntityLoadConfiguration(CultureLoadOption.Default, PropertyLoadOption.All, RelationLoadOption.None);

            var scriptToActiveContentRelationId = await script.GetRelationAsync<IParentToOneChildRelation>("ScriptToActiveScriptContent").ConfigureAwait(false);
            if (!scriptToActiveContentRelationId.Child.HasValue)
            {
                return null;
            }

            var activeContentEntity = await _mClientFactory.Client.Entities.GetAsync(scriptToActiveContentRelationId.Child.Value, activeContentLoadConfig);
            return activeContentEntity.GetPropertyValue<string>("M.ScriptContent.Script");
        }


        private async Task<long> CreateActionScript(LocalScriptData scriptInfo)
        {
            var scriptEntity = await _mClientFactory.Client.EntityFactory.CreateAsync("M.Script", CultureLoadOption.Default).ConfigureAwait(false);
            scriptEntity.Identifier = scriptInfo.ScriptIdentifier;
            scriptEntity.SetPropertyValue("M.Script.Name", scriptInfo.Name);
            scriptEntity.SetPropertyValue("M.Script.Type", scriptInfo.Type);
            scriptEntity.SetPropertyValue("M.Script.Restricted", true);
            scriptEntity.SetPropertyValue("M.Script.Enabled", true);
            scriptEntity.SetPropertyValue("M.Script.Description", scriptInfo.Description);

            var scriptId = await _mClientFactory.Client.Entities.SaveAsync(scriptEntity).ConfigureAwait(false);
            return scriptId;
        }

        private async Task<long?> CreateOrUpdateDraftScript(long scriptId, string scriptContent)
        {
            var url = $"/api/scripts/{scriptId}/draft";
            var response = await _mClientFactory.Client.Raw.PostAsync(url).ConfigureAwait(false);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var responseObject = JObject.Parse(responseContent);
                    var draftId = responseObject["draft_script_content_id"].Value<long>();
                    Logger.LogDebug($"Script draft is created, draft id: {draftId}. Updating draft content");

                    var draftEntity = await _mClientFactory.Client.Entities.GetAsync(
                        draftId,
                        new EntityLoadConfiguration(CultureLoadOption.Default, new PropertyLoadOption("M.ScriptContent.Script"), RelationLoadOption.None))
                        .ConfigureAwait(false);
                    draftEntity.SetPropertyValue("M.ScriptContent.Script", scriptContent);
                    await _mClientFactory.Client.Entities.SaveAsync(draftEntity).ConfigureAwait(false);

                    return draftId;

                default:
                    Logger.LogError($"Failed to make script draft. Status code: {response.StatusCode}, message: {response.Content}");
                    return null;
            }
        }

        private async Task<bool> CompileScript(long draftScriptId)
        {
            var url = $"/api/scripts/{draftScriptId}/compile";
            await _mClientFactory.Client.Raw.PostAsync(url).ConfigureAwait(false);

            var tries = 10;
            while (tries > 0)
            {
                tries--;
                await Task.Delay(2000).ConfigureAwait(false);

                var entity = await _mClientFactory.Client.Entities.GetAsync(
                    draftScriptId,
                    new EntityLoadConfiguration(CultureLoadOption.Default,
                    new PropertyLoadOption("M.ScriptContent.CompileStatus", "M.ScriptContent.CompilationMessage", "M.ScriptContent.CompilationErrors"),
                    RelationLoadOption.None))
                    .ConfigureAwait(false);

                var status = entity.GetPropertyValue<string>("M.ScriptContent.CompileStatus").ToUpperInvariant();

                var succeeded = status.Equals("SUCCESS");
                var errors = status.Equals("ERRORS");

                if (succeeded || errors)
                {
                    var compilationMessage = entity.GetPropertyValue<string>("M.ScriptContent.CompilationMessage");

                    if (errors)
                    {
                        Logger.LogError($"Script compilation failed, message: {compilationMessage}");
                        Logger.LogError($"Compilationerrors: {entity.GetPropertyValue<JToken>("M.ScriptContent.CompilationErrors")?.ToString()}");
                    }
                    else
                    {
                        Logger.LogDebug($"Script compiled, message: {compilationMessage}");
                    }

                    return succeeded;
                }
            }

            Logger.LogError("Compliation result is unknown, maximum number of checks exceeded");
            return false;
        }

        private async Task<bool> PublishScript(long draftScriptId, long parentScriptId)
        {
            var url = $"/api/scripts/{draftScriptId}/publish";
            var response = await _mClientFactory.Client.Raw.PostAsync(url).ConfigureAwait(false);

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    Logger.LogInformation("Script published");
                    return true;

                default:
                    Logger.LogError($"Script publishing failed. Status code: {response.StatusCode}, message: {response.Content}");
                    return false;
            }
        }

        private async Task CreateActionIfAbsent(LocalScriptData scriptInfo, long scriptId)
        {
            var actionIdentifier = $"Action.{scriptInfo.ScriptIdentifier}";
            var query = new Query()
            {
                Filter = new CompositeQueryFilter()
                {
                    Children = new QueryFilter[]
                    {
                        new DefinitionQueryFilter()
                        {
                            Name = "M.Action"
                        },
                        new IdentifierQueryFilter()
                        {
                            Value = actionIdentifier,
                            Operator = ComparisonOperator.Equals
                        }
                    },
                    CombineMethod = CompositeFilterOperator.And
                }
            };

            var queryResult = await _mClientFactory.Client.Querying.QueryIdsAsync(query).ConfigureAwait(false);
            if (!queryResult.Items.Any())
            {
                Logger.LogInformation($"Creating action with name: \"{scriptInfo.Name}\"");

                var action = await _mClientFactory.Client.EntityFactory.CreateAsync("M.Action", CultureLoadOption.Default).ConfigureAwait(false);
                action.Identifier = actionIdentifier;
                action.SetPropertyValue("ActionName", scriptInfo.Name);
                action.SetPropertyValue("Type", "ActionScript");
                action.SetPropertyValue("ActionExecutionScope", "Internal");

                var settings = new JObject();
                settings["script_identifier"] = scriptInfo.ScriptIdentifier;
                settings["restricted"] = true;
                action.SetPropertyValue("Settings", settings);

                var actionId = await _mClientFactory.Client.Entities.SaveAsync(action).ConfigureAwait(false);

                Logger.LogInformation($"Action created, id: {actionId}. name:{scriptInfo.Name}");
            }
            else
            {
                Logger.LogDebug($"Action already exists, id: {queryResult.Items.First()}");
            }
        }
        #endregion
    }
}
