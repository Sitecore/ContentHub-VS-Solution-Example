using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Base.Features.Base.Domain
{
    public class SetDeliverablesLifeCycleStatusResource : Stylelabs.M.Base.Web.Api.Models.Resource
    {
        [JsonProperty("entityid")]
        public long EntityId { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("statusRelation")]
        public string StatusRelation { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; }
        [JsonProperty("reasonProperty")]
        public string ReasonProperty { get; set; }
    }
}
