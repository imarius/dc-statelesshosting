using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using DnsClient;

namespace StatelessHosting.Controllers
{
    public class RequestHandlerController : Controller
    {
        private IDnsQueryResponse DNSQueryResponse { get; set; }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            var toSend = new HoldingDetails
            {
                DomainName = Request.Form["DomainName"],
                HoldingName = Request.Form["HoldingName"],
                PageColor = Request.Form["PageColor"],
                UrlToImage = Request.Form["UrlToImage"]
            };


            var domainSettings = await GetDomainSettingsAsync(toSend.DomainName);

            return Ok(domainSettings);
        }

        /// <summary>
        /// Populates DNSQueryResponse
        /// </summary>
        /// <param name="domainName">DomainName to be queried</param>
        public async void GetDNSRecordsAsync(string domainName)
        {
            var lookup = new LookupClient();

            DNSQueryResponse = await lookup.QueryAsync(domainName, QueryType.TXT);
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
            var client = new RestClient();
            client.BaseUrl = new System.Uri("https://domainconnect.api.godaddy.com/v2/");

            var request = new RestRequest(domainName + "/settings", Method.GET);
            var response = new RestResponse();

            response = await GetResponseContentAsync(client, request) as RestResponse;

            return JsonConvert.DeserializeObject<DomainSetting>(response.Content);
        }
    }
}