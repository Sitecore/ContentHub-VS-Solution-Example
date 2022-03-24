using System.Collections.Generic;

namespace Sitecore.CH.Base.CommandLine.Commands.Features.Scripting.Domain
{
    public class ScriptCollectionType
    {
        public string DirectoryName { get; set; }
        
        public string ScriptType { get; set; }

        public bool CanMapToAction { get; set; }

        public IList<LocalScriptData> Scripts { get; set; }
    }
}
