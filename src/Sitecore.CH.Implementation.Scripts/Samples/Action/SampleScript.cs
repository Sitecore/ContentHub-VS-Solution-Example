using Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Threading.Tasks;

namespace Sitecore.CH.Implementation.Scripts.Samples.Action
{
    /// <summary>
    /// Sample script
    /// </summary>
    public class SampleScript : BaseActionScriptScope
    {
        public SampleScript(IActionScriptContext context, IMClient client) : base(context, client)
        {

        }

        public override async Task Run()
        {
            var targetEntity = Context.Target as IEntity;

            string DefinitionName = "";
            const string EntityNotFound = "Entity has not been found";
            string UnexpectedDefinition(string definition, string expectedDefinition) => $"Received {definition} definition, expected {expectedDefinition} definition.";
            const string ExpectedDefinition = "M.Asset";
            if (!IsValid(targetEntity, ExpectedDefinition, EntityNotFound, UnexpectedDefinition))
            {
                return;
            }

            try
            {
                //apply transformations to entity at this point
                LogInfo("TestLog");


                targetEntity.SetPropertyValue("Title", "SampleScriptValue");

                var title = targetEntity.GetPropertyValue<string>("Title");

                LogInfo($"Asset Title - '{title}'");

                //Only required if we are on the post stage of a script
                if (Context.ExecutionPhase.HasValue && Context.ExecutionPhase == ExecutionPhase.Post)
                    await MClient.Entities.SaveAsync(targetEntity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message + " " + ex.StackTrace);
                throw;
            }

            void LogDebug(string message)
            {
                MClient.Logger.Debug($"{LogPrefix()}{message}");
            }
            void LogInfo(string message)
            {
                MClient.Logger.Info($"{LogPrefix()}{message}");
            }
            void LogWarn(string message)
            {
                MClient.Logger.Warn($"{LogPrefix()}{message}");
            }
            void LogError(Exception ex, string message = null)
            {
                if (message == null)
                    MClient.Logger.Error(LogPrefix(), ex);
                else
                    MClient.Logger.Error($"{LogPrefix()}{message}", ex);
            }
            string LogPrefix()
            {
                var id = Context.TargetId.HasValue ? Context.TargetId.Value.ToString() : "New";
                var identifier = (Context.Target as IEntity)?.Identifier;
                return $"{DefinitionName}|{id}:{identifier}|{Context.ExecutionPhase}|";
            }

            bool IsValid(IEntity entity, string expectedDefinition, string entityNotFoundMsg, Func<string, string, string> unexpectedDefinitionMsg)
            {
                if (entity == null)
                {
                    LogError(null, entityNotFoundMsg);
                    return false;
                }

                DefinitionName = entity.DefinitionName;
                if (entity.DefinitionName != expectedDefinition)
                {
                    LogWarn(unexpectedDefinitionMsg(entity.DefinitionName, expectedDefinition));
                    return false;
                }

                return true;
            }
            bool IsRelationDirty(IEntity entity, string relationName)
            {
                var relation = entity.GetRelation(relationName);
                return relation?.IsDirty == true || Context?.ChangeTracker?.IsRelationDirty(relationName) == true;
            }
            bool IsPropertyDirty(IEntity entity, string propertyName)
            {
                var property = entity.GetProperty(propertyName);
                return property?.IsDirty == true || Context?.ChangeTracker?.IsPropertyDirty(propertyName) == true;
            }
        }

    }
}
