using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sitecore.CH.Base.Features.Base.Domain
{
    public class SaveEntityMessage
    {
        [JsonProperty(PropertyName = "saveEntityMessage")]
        public SaveEntityMessageBody SaveEntityMessageBody { get; set; }
    }
    public class SaveEntityMessageBody
    {
        public string EventType { get; set; }

        public DateTime TimeStamp { get; set; }

        public bool IsNew { get; set; }

        public string TargetDefinition { get; set; }

        public long TargetId { get; set; }

        public string TargetIdentifier { get; set; }

        public DateTime CreatedOn { get; set; }

        public long UserId { get; set; }

        public int Version { get; set; }

        public ChangeSet ChangeSet { get; set; }
    }

    public class RelationChange
    {
        [JsonProperty(PropertyName = "Relation")]
        public string Relation { get; set; }

        [JsonProperty(PropertyName = "Role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "Cardinality")]
        public string Cardinality { get; set; }

        [JsonProperty(PropertyName = "NewValues")]
        public List<long> NewValues { get; set; }

        [JsonProperty(PropertyName = "RemovedValues")]
        public List<long> RemovedValues { get; set; }
    }

    public class PropertyChange
    {
        [JsonProperty(PropertyName = "Culture")]
        public string Culture { get; set; }

        [JsonProperty(PropertyName = "Property")]
        public string Property { get; set; }

        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "OriginalValue")]
        public string OriginalValue { get; set; }

        [JsonProperty(PropertyName = "NewValue")]
        public string NewValue { get; set; }
    }

    public class ChangeSet
    {
        [JsonProperty(PropertyName = "PropertyChanges")]
        public List<PropertyChange> PropertyChanges { get; set; }

        [JsonProperty(PropertyName = "Cultures")]
        public List<string> Cultures { get; set; }

        [JsonProperty(PropertyName = "RelationChanges")]
        public List<RelationChange> RelationChanges { get; set; }
    }
}
