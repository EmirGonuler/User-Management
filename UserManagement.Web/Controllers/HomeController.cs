using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UserManagement.Web.Models;

namespace UserManagement.Web.Controllers
{
    /// <summary>
    /// Handles application-level routing such as the error page.
    /// The default landing page redirects straight to the Users list.
    /// </summary>
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
