using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels;
using TSZH_Komarov.Viewmodels.Service;

namespace TSZH_Komarov.Controllers
{
    public class ServicesController : Controller
    {
        ServicesService servicesService;
        public ServicesController(ServicesService servicesService)
        {
            this.servicesService = servicesService;
        }

        public IActionResult Services(string filter = "all")
        {
            var services = servicesService.GetServiceView(filter);
            return View(services);
        }

        public IActionResult ServiceHistory(string filter = "all")
        {
            var services = servicesService.GetServiceView(filter);
            return View(services);
        }

        [HttpPost]
        public async Task<IActionResult> Services(int serviceTypeId, string? note)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Произошла ошибка! Попробуйте еще раз!";
                return RedirectToAction("Services", "Services");
            }
            string msg = servicesService.AddRequest(serviceTypeId, note);
           
            TempData["Message"] = msg;
            return RedirectToAction("Services", "Services");
        }


        public IActionResult ModerateServices()
        {
            var activeRequests = servicesService.LoadActiveRequests();

            return View(activeRequests);
        }

        [HttpPost]
        public async Task<IActionResult> ModerateServices(int requestId, string newStatus, string? adminComment)
        {
            string msg = servicesService.UpdateRequestStatus(requestId, newStatus, adminComment);

            TempData["Message"] = msg;

            return RedirectToAction("ModerateServices");
        }


    }
}
