using Newtonsoft.Json;

namespace StatelessHosting.Controllers
{
    public class DomainSetting
    {
        [JsonProperty(PropertyName = "providerName")]
        public string ProviderName { get; set; }
        [JsonProperty(PropertyName = "urlSyncUX")]
        public string UrlSyncUX { get; set; }
        [JsonProperty(PropertyName = "urlAsyncUx")]
        public string UrlAsyncUX { get; set; }
        [JsonProperty(PropertyName = "urlAPI")]
        public string UrlApi { get; set; }
    }
}