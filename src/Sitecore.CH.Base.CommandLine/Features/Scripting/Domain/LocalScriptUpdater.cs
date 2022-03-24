using Microsoft.Extensions.Options;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Config;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Domain
{
    public class LocalScriptUpdater
    {
        private Mapper _mapper;
        private string _scriptDirectory;

        public LocalScriptUpdater(IOptions<ScriptPushSettings> settingsOptions)
        {
            var settings = settingsOptions.Value;
            _mapper = new Mapper(settings.FolderToScriptPrefixMapping);
            _scriptDirectory = settings.ScriptDirectoryPath;
        }

        public void CreateOrUpdateLocalFile(EnvScriptData data)
        {
            var filePath = FindOrCreateFile(_scriptDirectory, data);
            UpdateScriptBody(filePath, data.ScriptBody);
        }

        private string FindOrCreateFile(string scriptDirectory, EnvScriptData data)
        {
            var parts = data.Name.Split(" - ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new Exception($"Script name has unknown structure: \"{data.Name}\"");
            }
            var shortGroupName = parts[0];
            var fileName = parts[1];

            var filePath = $"{scriptDirectory}\\{data.Type}\\{_mapper.GetGroupFullName(shortGroupName)}\\{fileName}.cs";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, Encoding.UTF8.GetString(GetActionScriptTemplate()));
            }
            return filePath;
        }

        private byte[] GetActionScriptTemplate()
        {
            var template = CommandLine.Features.Scripting.Resources.Scripting.ActionScriptTemplate;
            return template;
        }

        private void UpdateScriptBody(string filePath, string scriptBody)
        {
            scriptBody = RemoveUsings(scriptBody);

            var fileContent = File.ReadAllText(filePath);
            var start = FindScriptBodyStart(filePath, fileContent);
            var end = FindScriptBodyEnd(fileContent);
            var newFileContent = string.Concat(
                fileContent.Substring(0, start),
                scriptBody,
                fileContent.Substring(end)
                );

            File.WriteAllText(filePath, newFileContent);
        }

        private string RemoveUsings(string scriptBody)
        {
            var re = new Regex("^\\s*using[ ][^;]+;", RegexOptions.Multiline);
            return re.Replace(scriptBody, "");
        }

        private int FindScriptBodyStart(string filePath, string fileContent)
        {
            var idx = fileContent.IndexOf("Run()");
            if (idx == -1)
            {
                throw new Exception($"Failed to parse file: {filePath}. Script body is not found.");
            }

            return fileContent.IndexOf("{", idx) + 1;
        }

        private int FindScriptBodyEnd(string fileContent)
        {
            return fileContent.LastIndexOf("}", fileContent.Length - 1, 3);
        }
    }
}
