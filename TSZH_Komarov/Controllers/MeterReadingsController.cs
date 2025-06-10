using Microsoft.AspNetCore.Mvc;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels.Readings;

namespace TSZH_Komarov.Controllers
{
    public class MeterReadingsController : Controller
    {
        private MeterReadingsService meterReadingsService;
        private UserService userService;

        public MeterReadingsController(MeterReadingsService meterReadingsService, UserService userService)
        {
            this.meterReadingsService = meterReadingsService;
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult MeterReadings()
        {
            var model = meterReadingsService.GetMeters();
            return View(model);
        }

        [HttpGet]
        public IActionResult MeterReadingsHistory(int apartmentId)
        {
            var model = meterReadingsService.GetMeterHistory(apartmentId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitApartmentMeters(ApartmentMetersViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Проверьте введенные данные. Имеются ошибки.");

                var freshData = meterReadingsService.GetMeters();
                var apartmentIndex = freshData.FindIndex(a => a.ApartmentId == model.ApartmentId);
                if (apartmentIndex != -1)
                {
                    freshData[apartmentIndex] = model;
                }

                return View("MeterReadings", freshData);
            }

            var readings = model.Meters
                .Where(m => m.CurrentValue.HasValue)
                .Select(m => new MeterReadingViewModel
                {
                    ApartmentId = model.ApartmentId,
                    MeterTypeId = m.MeterTypeId,
                    CurrentValue = m.CurrentValue
                })
                .ToList();

            if (await meterReadingsService.AddMetersForApartment(model.ApartmentId, readings))
            {
                TempData["Message"] = $"Показания для квартиры {model.AccountNumber} отправлены!";
            }
            else
            {           
                var freshData = meterReadingsService.GetMeters();
                var apartmentIndex = freshData.FindIndex(a => a.ApartmentId == model.ApartmentId);
                if (apartmentIndex != -1)
                {
                    freshData[apartmentIndex] = model;
                }

                return View("MeterReadings", freshData);
            }

            return RedirectToAction(nameof(MeterReadings));
        }
    }
}
