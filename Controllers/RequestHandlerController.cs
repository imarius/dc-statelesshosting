using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DnsClient;
using System.Net;
using StatelessHosting.Models;
using Microsoft.AspNetCore.Http;

namespace StatelessHosting.Controllers
{
    public class RequestHandlerController : Controller
    {
        private IDnsQueryResponse DNSQueryResponse { get; set; }

        private DomainSetting DomainSettings { get; set; }

        private string APIUrl { get; set; }

        private HoldingDetails PageDetails { get; set; }

        private string HostName
        {
            get
            {
                HostString host = HttpContext.Request.Host;
                return host.Host;
            }
        }
        
        private const string ClientId = "whdhackathon";
        private const string Secret = "DomainConnectGeheimnisSecretString";
        private const string Scope = "whd-template-1";

        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var _method = HttpContext.Request.Method;


            if (_method == "GET")
                return Redirect("/");


            PageDetails = new HoldingDetails
            {
                DomainName = Request.Form["DomainName"],
                HoldingName = Request.Form["HoldingName"],
                PageColor = Request.Form["PageColor"],
                UrlToImage = Request.Form["UrlToImage"]
            };

            var base64PageDetails = Base64Encode(JsonConvert.SerializeObject(PageDetails));
            // Try and find TXT record containing a URL used as a prefix            
            await GetDNSRecordsAsync(PageDetails.DomainName);

            DomainSettings = await GetDomainSettingsAsync(PageDetails.DomainName);
            var base64DSettings = Base64Encode(JsonConvert.SerializeObject(DomainSettings));
            
            var serializedJson = JsonConvert.SerializeObject(PageDetails);
            var base64EncodedString = Base64Encode(serializedJson);

            ViewBag.provider = DomainSettings.ProviderName;

            ViewBag.url = DomainSettings.UrlSyncUX + "/v2/domainTemplates/providers/whdhackathon/services/whd-template-1/apply?domain="
                    + PageDetails.DomainName + "&RANDOMTEXT=" + base64EncodedString + "&IP=127.0.0.1";

            ViewBag.urlasync = "/v2/domainTemplates/providers/WHDHackathon/services/whd-template-1?domain=" +
                 PageDetails.DomainName + "&client_id=" + ClientId + "&redirect_url=http://" + HostName
                 + "/RequestHandler/AsyncCallback&scope=whd-template-1";

            return View("Callback");
        }

        [HttpGet]
        public ActionResult AsyncCallback(string code)
        {
            // PageDetails = JsonConvert.DeserializeObject<HoldingDetails>(Base64Decode(appConf));
            // DomainSettings = JsonConvert.DeserializeObject<DomainSetting>(Base64Decode(dsets));

            // var endpoint = "/v2/domainTemplates/providers/WHDHackathon/services/whd-template-1?domain=" +
            //      PageDetails.DomainName + "&client_id=" + ClientId + "&redirect_url=http://" + HostName
            //      + "/RequestHandler/Async&scope=whd-template-1";
            
            // return Redirect(DomainSettings.UrlAsyncUX + endpoint);

            return Ok("hello");

        }

        public ActionResult Async(string code)
        {
            var inboundCode = code;

            return Ok("I GOT HERE");
        }
        /// <summary>
        /// Populates DNSQueryResponse
        /// </summary>
        /// <param name="domainName">DomainName to be queried</param>
        public async Task GetDNSRecordsAsync(string domainName)
        {
            var lookup = new LookupClient(IPAddress.Parse("8.8.8.8"));

            DNSQueryResponse = await lookup.QueryAsync("_domainconnect." + domainName, QueryType.TXT);

            var _txtRecord = string.Empty;
            foreach (var _answer in DNSQueryResponse.Answers)
            {
                _txtRecord = _answer.RecordToString();
            }

            DNSQueryResponse = await lookup.QueryAsync("_domainconnect." + domainName, QueryType.CNAME);

            var _cnameRecord = string.Empty;
            foreach (var _answer in DNSQueryResponse.Answers)
            {
                _cnameRecord = _answer.RecordToString();
            }

            //GET Text Record of the CNAME

            if (!string.IsNullOrEmpty(_cnameRecord))
            {
                DNSQueryResponse = await lookup.QueryAsync(_cnameRecord, QueryType.TXT);

                foreach (var _answer in DNSQueryResponse.Answers)
                {
                    APIUrl = _answer.RecordToString();
                }
            } else if (!string.IsNullOrEmpty(_txtRecord))
            {
                APIUrl = _txtRecord;
            }
        }

        /// <summary>
        /// Gets Response from the Client
        /// </summary>
        /// <param name="theClient">RestSharp Client</param>
        /// <param name="theRequest">The request Object</param>
        /// <returns>Task that return the response</returns>
        private static Task<IRestResponse> GetResponseContentAsync(RestClient theClient, RestRequest theRequest)
        {
            var tcs = new TaskCompletionSource<IRestResponse>();
            theClient.ExecuteAsync(theRequest, response =>
            {
                tcs.SetResult(response);
            });
            return tcs.Task;
        }

        /// <summary>
        /// Returns the DomainSettings Object
        /// </summary>
        /// <param name="domainName">DomainName to be interogated.</param>
        /// <returns></returns>
        private async Task<DomainSetting> GetDomainSettingsAsync(string domainName)
        {
            //Sanitize API URL
            var _providerEndpoint = APIUrl.Replace('"', ' ');
            _providerEndpoint = _providerEndpoint.Trim();

            if (!_providerEndpoint.Contains("https"))
                _providerEndpoint = "https://" + _providerEndpoint;

            var client = new RestClient(_providerEndpoint);

            var request = new RestRequest("/v2/" + domainName + "/settings", Method.GET);
            var response = new RestResponse();

            response = await GetResponseContentAsync(client, request) as RestResponse;

            return JsonConvert.DeserializeObject<DomainSetting>(response.Content);
        }

        private async Task<DomainSetting> GetAsyncEndPoint(string urlToCall)
        {
            var client = new RestClient(DomainSettings.UrlAsyncUX);

            var request = new RestRequest(urlToCall, Method.GET);
            var response = new RestResponse();

            response = await GetResponseContentAsync(client, request) as RestResponse;
            var variable = response.Content;
            return new DomainSetting();
        }

        /// <summary>
        /// Encode to a BASE64 String
        /// </summary>
        /// <param name="plainText">Plain text</param>
        /// <returns>Base64 Encoded String</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decode a Base64 String 
        /// </summary>
        /// <param name="base64EncodedData">Base64 encoded string</param>
        /// <returns>Plain Text String</returns>
        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}