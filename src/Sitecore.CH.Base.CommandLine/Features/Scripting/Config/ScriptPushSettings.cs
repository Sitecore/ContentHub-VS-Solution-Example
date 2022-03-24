using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Config
{
    public class ScriptPushSettings
    {
        [JsonProperty("scriptDirectoryPath")]
        public string ScriptDirectoryPath { get; set; }

        [JsonProperty("additionalNamespaces")]
        public string[] AdditionalNamespaces { get; set; }

        [JsonProperty("excludedNamespaces")]
        public string[] ExcludedNamespaces { get; set; }

        [JsonProperty("folderToScriptPrefixMapping")]
        public Dictionary<string, string> FolderToScriptPrefixMapping { get; set; }


    }
}

