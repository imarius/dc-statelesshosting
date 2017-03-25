using Microsoft.AspNetCore.Mvc;

namespace StatelessHosting.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            string method = HttpContext.Request.Method;

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
    }
}
