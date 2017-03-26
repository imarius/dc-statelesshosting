using Newtonsoft.Json;

namespace StatelessHosting.Models
{
    public class HoldingDetails 
    {
        [JsonProperty(PropertyName = "holdingName")]
        public string HoldingName {get; set;}
        [JsonProperty(PropertyName = "pageColor")]        
        public string PageColor {get; set;}
        [JsonProperty(PropertyName = "imageUrl")]        
        public string UrlToImage {get; set;}
        [JsonProperty(PropertyName = "domainName")]        
        public string DomainName {get; set;}
    }
}