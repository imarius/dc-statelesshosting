using System.Net;
using DnsClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StatelessHosting.Models;

namespace StatelessHosting.Controllers
{
    public class HomeController : Controller
    {
        private IDnsQueryResponse DNSQueryResponse { get; set; }
        public IActionResult Index()
        {
            string method = HttpContext.Request.Method;

            HostString host = HttpContext.Request.Host;
            string hostname = host.Host;
            var lookup = new LookupClient(IPAddress.Parse("8.8.8.8"));
            string txtName = string.Empty;

            DNSQueryResponse = lookup.Query(hostname, QueryType.TXT);
            foreach (var _answer in DNSQueryResponse.Answers)
            {
                var domainTxtName = _answer.RecordToString();
                var response = domainTxtName.Substring(1, domainTxtName.Length - 2).Replace(@"\/", "/");
                txtName = Base64Decode(response);
            }

            //Return normal view when GET is called.
            if (method == "GET")
                return View();

            //Populate holding object

            var holdingPage = new HoldingDetails();
            holdingPage.DomainName = "";

            var holdingThisTest = Request.Body;

            //Just return a simple response write.
            return null;
        }

        /// <summary>
        /// Decode a Base64 String 
        /// </summary>
        /// <param name="base64EncodedData">Base64 encoded string</param>
        /// <returns>Plain Text String</returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
