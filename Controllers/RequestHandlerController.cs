using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DnsClient;
using System.Net;
using StatelessHosting.Models;

namespace StatelessHosting.Controllers
{
    public class RequestHandlerController : Controller
    {
        private IDnsQueryResponse DNSQueryResponse { get; set; }

        private DomainSetting DomainSettings { get; set; }

        private string APIUrl { get; set; }

        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var _method = HttpContext.Request.Method;

            if (_method == "GET")
                return Redirect("/");


            var toSend = new HoldingDetails
            {
                DomainName = Request.Form["DomainName"],
                HoldingName = Request.Form["HoldingName"],
                PageColor = Request.Form["PageColor"],
                UrlToImage = Request.Form["UrlToImage"]
            };

            // Try and find TXT record containing a URL used as a prefix            
            await GetDNSRecordsAsync(toSend.DomainName);

            DomainSettings = await GetDomainSettingsAsync(toSend.DomainName);

            var serializedJson = JsonConvert.SerializeObject(toSend);
            var base64EncodedString = Base64Encode(serializedJson);

            ViewBag.provider = DomainSettings.ProviderName;

            ViewBag.url = DomainSettings.UrlSyncUX + "/v2/domainTemplates/providers/WHDHackathon/services/whd-template-1/apply?domain="
                    + toSend.DomainName + "&RANDOMTEXT=" + base64EncodedString + "&IP=127.0.0.1";

            return View("Callback");
        }

        /// <summary>
        /// Populates DNSQueryResponse
        /// </summary>
        /// <param name="domainName">DomainName to be queried</param>
        public async Task GetDNSRecordsAsync(string domainName)
        {
            var lookup = new LookupClient(IPAddress.Parse("8.8.8.8"));

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
            _providerEndpoint = "https://" + _providerEndpoint;

            var client = new RestClient(_providerEndpoint);
            client.BaseUrl = new System.Uri(_providerEndpoint);

            var request = new RestRequest("/v2/" + domainName + "/settings", Method.GET);
            var response = new RestResponse();

            response = await GetResponseContentAsync(client, request) as RestResponse;

            return JsonConvert.DeserializeObject<DomainSetting>(response.Content);
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
    }
}