using Newtonsoft.Json;
using Stylelabs.M.Base.Web.Api.Models;
using System;
using System.Collections.Generic;

namespace Sitecore.CH.Base.Features.Base.Domain
{
    public class BusinessAuditQuery
    {
        [JsonProperty(PropertyName = "items")]
        public List<BusinessAuditLogEntry> Items { get; set; }
        [JsonProperty(PropertyName = "total_items")]
        public int TotalItems { get; set; }
        [JsonProperty(PropertyName = "returned_items")]
        public int ReturnedItems { get; set; }
        [JsonProperty(PropertyName = "next")]
        public Link Next { get; set; }
    }

    public class BusinessAuditLogEntry
    {
        [JsonProperty(PropertyName = "event_type")]
        public string Eventype { get; set; }
        [JsonProperty(PropertyName = "target_id")]
        public string TargetId { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime? TimeStamp { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public long? UserId { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
    }
}

