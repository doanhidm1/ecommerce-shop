using Demo.DependencyInjections;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class DIController : Controller
    {
        private readonly IServiceA _serviceA;
        private readonly IServiceA _serviceA1;
        private readonly IServiceA _serviceA2;


        public DIController(IServiceA serviceA, IServiceA serviceA1, IServiceA serviceA2)
        {
            _serviceA = serviceA;
            _serviceA1 = serviceA1;
            _serviceA2 = serviceA2;
        }
  
        public IActionResult Index()
        {
            ViewBag.IdA = _serviceA.GetId();
            ViewBag.IdA1 = _serviceA1.GetId();
            ViewBag.IdA2 = _serviceA2.GetId();
            return View();
        }
    }
}
