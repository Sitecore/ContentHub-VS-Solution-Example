using Newtonsoft.Json;

namespace Sitecore.CH.Base.Features.Base.Domain
{
    public class SetMasterCommandResource
    {
        [JsonProperty("new_master_id")]
        public long? NewMasterId { get; set; }

        [JsonProperty("entity_id")]
        public long EntityId { get; set; }

        [JsonProperty("master_relation")]
        public string MasterRelation { get; set; }
    }
}
