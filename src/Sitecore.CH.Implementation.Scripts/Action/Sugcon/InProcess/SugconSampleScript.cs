using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Sdk;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Sitecore.CH.Implementation.Scripts.Features.BaseScriptScope;

namespace Sitecore.CH.Implementation.Scripts.Action
{
    /// <summary>
    /// Sample script to set AssetTypeToAsset relation (M.Asset + M.AssetType
    /// </summary>
    public class SugconSampleScript : BaseActionScriptScope
    {
        public SugconSampleScript(IActionScriptContext context, IMClient client) : base(context, client)
        {

        }

        public override async Task Run()
        {
            var targetEntity = Context.Target as IEntity;

            string DefinitionName = "";
            long? assetTypeId;
            const string EntityNotFound = "Entity has not been found";
            string UnexpectedDefinition(string definition, string expectedDefinition) => $"Received {definition} definition, expected {expectedDefinition} definition.";
            const string ExpectedDefinition = "M.Asset";

            const string AssetTypeVideoIdentifier = "M.AssetType.Video"; 
            const string AssetTypeDocumentIdentifier = "M.AssetType.Document"; 
            const string AssetTypeImageIdentifier = "M.AssetType.Image"; 
            const string AssetTypeGenericIdentifier = "M.AssetType.Generic";

            const string AssetTypeToAssetRelation = "AssetTypeToAsset";
            const string AssetFileNameProperty = "FileName";

            List<string> ImageFileTypes = new List<string>() { ".png", ".jpg",".jpeg" }; 
            List<string> DocumentFileTypes = new List<string>() { ".pdf",".doc",".docx" }; 
            List<string> VideoFileTypes = new List<string>() { ".mpeg",".avi",".mp4" }; 


            if (!IsValid(targetEntity, ExpectedDefinition, EntityNotFound, UnexpectedDefinition))
            {
               return;
            }

            try
            {
                //apply transformations to entity at this point
                LogDebug($"Sugcon Script - Start for Entity {targetEntity.Identifier}");
                
                //get FileName
                var assetFileName = targetEntity.GetPropertyValue<string>(AssetFileNameProperty);

                //var assetFileExtension = getFileExtension(assetFileName);
                var assetFileExtension = Path.GetExtension(assetFileName); //Showcase2: Build Error
                
                
                //set Asset Type according to filetype of asset being uploaded
    
                if (ImageFileTypes.Contains(assetFileExtension))
                    assetTypeId = await getAssetTypeId(AssetTypeImageIdentifier).ConfigureAwait(false);
                else if (DocumentFileTypes.Contains(assetFileExtension))
                    assetTypeId = await getAssetTypeId(AssetTypeDocumentIdentifier).ConfigureAwait(false);
                else if (VideoFileTypes.Contains(assetFileExtension))
                    assetTypeId = await getAssetTypeId(AssetTypeVideoIdentifier).ConfigureAwait(false);
                else
                    assetTypeId = await getAssetTypeId(AssetTypeGenericIdentifier).ConfigureAwait(false);
                
                if (assetTypeId == null)
                    return;
          
                targetEntity.GetRelation(AssetTypeToAssetRelation).SetIds(new List<long>() { assetTypeId.Value });

                LogDebug($"Sugcon Script - Before Save - AssetType: {targetEntity.GetRelation(AssetTypeToAssetRelation).GetIds().FirstOrDefault()}");

                //Only required if we are on the post stage of a script
                if (Context.ExecutionPhase.HasValue && Context.ExecutionPhase == ExecutionPhase.Post)
                {
                    LogDebug("Sugcon Script - Save Changes");
                    await MClient.Entities.SaveAsync(targetEntity).ConfigureAwait(false);
                }
                
            }
            catch (Exception ex)
            {
                LogError(ex, ex.Message + " " + ex.StackTrace);
                throw;
            }

            //methods
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

            async Task<long?> getAssetTypeId(string AssetTypeIdentifier)
            {
                
                var assetType = await MClient.Entities.GetAsync(AssetTypeIdentifier, EntityLoadConfiguration.Minimal).ConfigureAwait(false);
                
                if (assetType == null)
                    LogWarn($"SUGCON-Sample: AssetType {AssetTypeIdentifier} does not exist");
                
                assetTypeId = assetType.Id;

                return assetTypeId;
            }

            string getFileExtension(string path)
            {
                int i = path.LastIndexOf('.');
                int length = path.Length - i;
                string fileExtension = path.Substring(i,length);

                return fileExtension;
            }
        }

    }
}
