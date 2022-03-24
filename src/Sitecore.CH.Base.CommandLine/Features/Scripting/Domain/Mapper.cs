using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Domain
{
    public class Mapper
    {
        private readonly Dictionary<string, string> _folderToScriptPrefixMapping;
        public Mapper(Dictionary<string, string> folderToScriptPrefixMapping)
        {
            _folderToScriptPrefixMapping = folderToScriptPrefixMapping;
        }

        public string GetGroupFullName(string groupShortName)
        {
            foreach(var item in _folderToScriptPrefixMapping)
            {
                if (item.Value == groupShortName)
                    return item.Key;
            }

            throw new Exception("Unknown group name, please update mapping");
        }

        public string GetIdentifier(string filePath, string scriptBody)
        {
            var scriptNamespace = GetScriptTokenValue(scriptBody, "namespace", string.Empty);
            var fileName = Path.GetFileName(filePath);
            string identifier = GetScriptTokenValue(scriptBody, "//Identifier:", string.Concat(scriptNamespace, ".", fileName.Replace(".cs", string.Empty)));
            return identifier;
        }

        public string GetScriptName(string filePath, string scriptBody)
        {
            var fileName = Path.GetFileName(filePath);
            return GetScriptTokenValue(scriptBody, "//Name:", fileName.Replace(".cs", string.Empty));
        }

        public string GetScriptDescription(string filePath, string scriptBody)
        {
            var fileName = Path.GetFileName(filePath);
            return GetScriptTokenValue(scriptBody, "//Description:", fileName.Replace(".cs", string.Empty));
        }

        private string GetScriptTokenValue(string scriptContent, string token, string defaultValue)
        {
            var scriptDescPatternWithCarriage = $"(?<={token} ).*?(?=\r\n)";
            var scriptDescPatternWithoutCarriage = $"(?<={token} ).*?(?=\n)";

            var scriptNameMatch = Regex.Match(scriptContent, scriptDescPatternWithCarriage);
            if (scriptNameMatch.Success && scriptNameMatch.Groups?.Count > 0)
                return scriptNameMatch.Groups[0].Value;

            scriptNameMatch = Regex.Match(scriptContent, scriptDescPatternWithoutCarriage);
            if (scriptNameMatch.Success && scriptNameMatch.Groups?.Count > 0)
                return scriptNameMatch.Groups[0].Value;

            return defaultValue;
        }
    }
}
