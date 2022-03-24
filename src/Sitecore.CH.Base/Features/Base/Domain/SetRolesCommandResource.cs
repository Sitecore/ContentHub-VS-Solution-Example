using Newtonsoft.Json;
using Stylelabs.M.Base.Web.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitecore.CH.Base.Features.Base.Domain
{
    public class SetRolesCommandResource : EntityRolesResource
    {
        [JsonProperty("target_id")]
        public long TargetId { get; set; }
    }
}
