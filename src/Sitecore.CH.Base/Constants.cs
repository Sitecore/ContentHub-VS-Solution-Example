namespace Sitecore.CH.Base
{
    public static class Constants
    {
        public static class EntityDefinitions
        {
            public static class Asset
            {
                public const string DefinitionName = Stylelabs.M.Sdk.Constants.Asset.DefinitionName;

                public static class Properties
                {
                    public const string Title = Stylelabs.M.Sdk.Constants.Asset.Title;
                    public const string FileName = Stylelabs.M.Sdk.Constants.Asset.FileName;
                }

                public static class Relations
                {
                    public const string AssetTypeToAsset = Stylelabs.M.Sdk.Constants.Asset.AssetTypeToAsset;
                }
            }

            public static class AssetType
            {
                public const string DefinitionName = Stylelabs.M.Sdk.Constants.AssetType.DefinitionName;
                                
                public static class Properties
                {
                    

                }

                public static class Relations
                {

                }
            }

            public static class Script
            {
                public const string DefinitionName = "M.Script";

                public static class Relations
                {
                    public const string ActionToScript = "ActionToScript";
                }
            }

            public static class Action
            {
                public const string DefinitionName = "M.Action";

                public static class Relations
                {
                    public const string ActionToScript = Script.Relations.ActionToScript;
                    public const string TriggerToAction = "TriggerToAction";
                    public const string TriggerToPreAction = "TriggerToPreAction";
                    public const string TriggerToSecurityPreAction = "TriggerToSecurityPreAction";
                    public const string TriggerToValidationPreAction = "TriggerToValidationPreAction";
                    public const string TriggerToAuditPreAction = "TriggerToAuditPreAction";
                }
            }

            public static class Trigger
            {
                public const string DefinitionName = "M.Trigger";

                public static class Properties
                {
                    public const string Name = "Name";
                    public const string IsEnabled = "IsEnabled";
                }

                public static class Relations
                {
                    public const string TriggerToAction = Action.Relations.TriggerToAction;
                    public const string TriggerToPreAction = Action.Relations.TriggerToPreAction;
                    public const string TriggerToSecurityPreAction = Action.Relations.TriggerToSecurityPreAction;
                    public const string TriggerToValidationPreAction = Action.Relations.TriggerToValidationPreAction;
                    public const string TriggerToAuditPreAction = Action.Relations.TriggerToAuditPreAction;
                }
            }

            public static class RoleTemplate
            {
                public const string DefinitionName = "M.RoleTemplate";

                public static class Properties
                {
                    public const string RoleName = "RoleName";
                    public const string Template = "Template";

                }
            }

            public static class User
            {
                public const string DefinitionName = Stylelabs.M.Sdk.Constants.User.DefinitionName;

                public static class Properties
                {
                    public const string Username = Stylelabs.M.Sdk.Constants.User.Username;
                }
                public static class Relations
                {
                    public const string UserGroupToUser = Stylelabs.M.Sdk.Constants.User.UserGroupToUser;
                }
            }

            public static class UserGroup
            {
                public const string DefinitionName = Stylelabs.M.Sdk.Constants.UserGroup.DefinitionName;

                public static class Properties
                {
                    public const string GroupName = Stylelabs.M.Sdk.Constants.UserGroup.GroupName;
                }

                public static class Values
                {
                    public const string Superusers = "Superusers";
                    public const string Everyone = "Everyone";
                }
            }

        }
        public static class Identifiers
        {
            public const string MContentRepositoryStandard = "M.Content.Repository.Standard";
            public const string MContentRepositoryProject = "M.Content.Repository.Project";
        }
        public static class Config
        {
            public static class Sections
            {
                public const string MSection = "M";
            }
        }

        public static class ScriptPropertyBagKeys
        {
            public const string ValidationFailuresJArray = "ValidationFailuresJArray";
        }

        public static class Logging
        {
            public const string UnableToGetAssetMediaQuery = "Unable to get Asset media query";
            public static string EntityHasBeenFound(long entityId) => $"Entity: {entityId}, has been found";
            public static string EntityHasNotBeenFound(string entityIdentifier) => $"Entity: {entityIdentifier}, has not been found";
            public static string IdentifierDoesNotExist(string identifier) => $"Identifier: {identifier} does not exist";
            public static string GotXEntitiesOf(int count, long totalResults) => $"Got {count}/{totalResults} entities";

            public static string BeginOperation(string operationName) => $"Begin {operationName}";
            public static string DoneOperation(string operationName) => $"Done {operationName}";
        }
        public static class Relations
        {
            public const string AssetMediaToAsset = "AssetMediaToAsset";

            public const string ContentRepositoryToAsset = "ContentRepositoryToAsset";
        }

        public static class Commands
        {
            public static class NameSpace
            {
                public static string Security = "m.security";
                public static string Project = "project";
                public static string Asset = "m.asset";
            }
            public static class Command
            {
                public static string SetRoles = "setroles";
                public static string SetMaster = "setmaster";
                public static string SetDeliverablesLifeCycleStatus = "set.deliverables.lifecyclestatus";
            }
            public static class args
            {
                public static string master_relation = "MasterFile";

            }
        }

        public static class Api
        {
            public static class AllRolesForEntity
            {
                public const string TemplateName = "all_roles_for_entity";
                public const string id = "id";
            }

            public static class CopyEntity
            {
                public const string TemplateName = "copy";
                public const string id = "id";
            }

            public static class BusinessAudit
            {
                public const string Endpoint = "/api/audit/business/";
                public const string Template = "{/entityId}{?from,to,fullText,take,sort,order,scrollId,scrollTime,raw}";
            }
        }

        public static class Collection
        {
            public static class Roles
            {
                public const string CollectionReader = "CollectionReader";
                public const string CollectionContributor = "CollectionContributor";
                public const string CollectionManager = "CollectionManager";
            }
        }
    }
}