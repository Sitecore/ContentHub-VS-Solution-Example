using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Config;
using Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Domain;
using Sitecore.CH.Base.Features.Logging.Services;
using Stylelabs.M.Scripting.Types.V1_0.Action;
using Stylelabs.M.Scripting.Types.V1_0.User.SignIn;
using Stylelabs.M.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Services
{
    public class LocalScriptDiscoveryService
    {

        private readonly ILoggerService<LocalScriptDiscoveryService> Logger;
        public LocalScriptDiscoveryService(ILoggerService<LocalScriptDiscoveryService> logger, IOptions<ScriptPushSettings> settings)
        {
            this.Logger = logger;
            this._settings = settings.Value;
        }
        private ScriptPushSettings _settings;
        private const string ScriptBodyMainFunctionName = "Run()";
        private const string MainClassStartString = " class ";
        private const string UsingKeywork = "using";

        public List<LocalScriptData> FindActionScripts(ScriptCollectionType scriptCollection)
        {
            var mapper = new Mapper(_settings.FolderToScriptPrefixMapping);

            var scripts = new List<LocalScriptData>();

            var directoryFullPath = Path.Combine(_settings.ScriptDirectoryPath, scriptCollection.DirectoryName);
            if (!Directory.Exists(directoryFullPath))
            {
                Logger.LogWarning($"Directory {directoryFullPath} not found.");
                return null;
            }

            AddFilesFromDirectory(mapper, scripts, scriptCollection.ScriptType, directoryFullPath);

            return scripts;
        }

        private void AddFilesFromDirectory(Mapper mapper, List<LocalScriptData> scriptsCollection, string scriptType, string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var script = GetScript(file, mapper);
                scriptsCollection.Add(new LocalScriptData()
                {
                    ScriptIdentifier = script.Identifier,
                    Type = scriptType,
                    Name = script.Name,
                    Description = script.Description,
                    ScriptBody = script.Body
                });
            }
        }

        private Script GetScript(string filePath, Mapper mapper)
        {
            bool mainClassStartFound = false;
            bool globalVariablesDone = false;

            bool scriptBodyStartFound = false;
            bool scriptBodyEndFound = false;
            var scriptContentLines = File.ReadAllLines(filePath);
            var scriptHeaders = new StringBuilder();
            var globalVariables = new StringBuilder();
            var scriptBodyBuilder = new StringBuilder();
            var bracesStack = new Stack<int>();
            int stackNo = 0;
            var scriptUsings = new List<string>();

            foreach (var line in scriptContentLines)
            {
                if (!mainClassStartFound && line.Contains(MainClassStartString))
                {
                    mainClassStartFound = true;
                    continue;
                }

                if (!scriptBodyStartFound && line.Contains(ScriptBodyMainFunctionName))
                {
                    globalVariablesDone = true;
                    scriptBodyStartFound = true;
                }

                if (mainClassStartFound && !scriptBodyStartFound && !globalVariablesDone)
                {
                    if ((line.Contains(nameof(IActionScriptContext)) ||
                        line.Contains(nameof(IUserSignInContext)))
                        && line.Contains(nameof(IMClient)))
                    {
                        globalVariablesDone = true;
                        continue;
                    }

                    globalVariables.Append(TrimStart(line, ' ', 8));
                    globalVariables.Append(Environment.NewLine);
                    continue;
                }

                //anything above main function
                if (!scriptBodyStartFound || line.Contains(ScriptBodyMainFunctionName))
                {
                    scriptHeaders.Append(line);
                    scriptHeaders.Append(Environment.NewLine);

                    if (line.StartsWith(UsingKeywork))
                        scriptUsings.Add(line);

                    continue;
                }

                string lineToAdd = line;
                if (!scriptBodyEndFound) //detect if current line is end of main function so closing brace is not added
                {
                    var numberOfOpenBraces = line.Length - line.Replace("{", "").Length;
                    var numberOfClosingBraces = line.Length - line.Replace("}", "").Length;

                    for (int i = 0; i < numberOfOpenBraces; i++)
                        bracesStack.Push(stackNo++);

                    for (int i = 0; i < numberOfClosingBraces; i++)
                        bracesStack.Pop();

                    if (!bracesStack.Any()) //end of scripbody
                    {
                        scriptBodyEndFound = true;
                        continue;
                    }

                    lineToAdd = TrimStart(lineToAdd, ' ', 4);
                }

                lineToAdd = TrimStart(lineToAdd, ' ', 8);
                scriptBodyBuilder.Append(lineToAdd);
                scriptBodyBuilder.Append(Environment.NewLine);
            }

            var scriptBody = scriptBodyBuilder.ToString();
            scriptBody = TrimStart(scriptBody, '{', 1);
            scriptBody = TrimEnd(scriptBody, '}', 2);
            globalVariables.Append(scriptBody);
            scriptBody = TrimStart(globalVariables.ToString(), '{', 1);
            scriptBody = PrependNamespaces(scriptBody, scriptUsings); //add namespaces from settings json file

            if (!scriptBodyStartFound)
            {
                throw new Exception($"Failed to parse file: {filePath}. Script body is not found.");
            }

            var fileHeaders = scriptHeaders.ToString();
            var script = new Script
            {
                Identifier = mapper.GetIdentifier(filePath, fileHeaders),
                Name = mapper.GetScriptName(filePath, fileHeaders),
                Description = mapper.GetScriptDescription(filePath, fileHeaders),
                Body = scriptBody
            };

            return script;
        }

        private string TrimStart(string text, char charToTrim, int count)
        {
            while (count > 0)
            {
                text = text.Substring(text.IndexOf(charToTrim) + 1);
                count--;
            }
            return text;
        }

        private string TrimEnd(string text, char charToTrim, int count)
        {
            while (count > 0)
            {
                text = text.Substring(0, text.LastIndexOf(charToTrim));
                count--;
            }
            return text;
        }

        private string PrependNamespaces(string scriptBody, List<string> scriptUsings)
        {
            var fullScriptBody = new StringBuilder();
            var usingsToAdd = new List<string>();
            usingsToAdd.AddRange(scriptUsings);
            if (_settings.AdditionalNamespaces != null)
                foreach (var additionalNamespace in _settings.AdditionalNamespaces)
                {
                    var usingClause = $"{UsingKeywork} {additionalNamespace};";

                    if (scriptBody.IndexOf(usingClause, StringComparison.InvariantCulture) == -1)
                    {
                        fullScriptBody.AppendLine(usingClause);
                        usingsToAdd.Remove(usingClause);
                    }
                }

            if (_settings.ExcludedNamespaces != null)
                foreach (var excludedNamespaces in _settings.ExcludedNamespaces)
                {
                    usingsToAdd.RemoveAll(s => s.EndsWith($"{excludedNamespaces};"));
                }

            usingsToAdd.ForEach((@using) =>
            {
                fullScriptBody.AppendLine(@using);
            });


            if (fullScriptBody.Length <= 0)
                return scriptBody;

            fullScriptBody.AppendLine();
            fullScriptBody.Append(scriptBody);
            return fullScriptBody.ToString();
        }
    }
}
