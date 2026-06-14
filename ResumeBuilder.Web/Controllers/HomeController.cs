using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ResumeBuilder.Web.Models;

namespace ResumeBuilder.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        if (statusCode == 401 || statusCode == 403)
        {
            return View("AuthorizationFailed");
        }
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, StatusCode = statusCode });
    }
}
