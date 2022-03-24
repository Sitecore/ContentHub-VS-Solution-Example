using System.Collections.Generic;

namespace Sitecore.CH.Base.Features.SDK.Services.Config
{
    public class MClientOptions
    {
        public string Host { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<string> KnownSSoRedirects { get; set; } = new List<string>();
    }
}
