using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TSZH_Komarov.Data;
using TSZH_Komarov.Services;

namespace TSZH_Komarov.Components
{
    public class ApartmentSwitcherViewComponent : ViewComponent
    {
        private readonly TszhKomarovContext context;
        private readonly UserService userService;

        public ApartmentSwitcherViewComponent(TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = userService.GetCurrUser();
            var currentTszhId = userService.GetCurrTszh().TszhId;

            var currentAppartmentId = UserClaimsPrincipal.FindFirstValue("appartment");

            var apartments = await context.Apartments
                .Where(a => a.UserId == user.UserId && a.House.TszhId == currentTszhId)
                .Select(a => new ApartmentSwitcherItem
                {
                    ApartmentId = a.ApartmentId,
                    Number = a.Number,
                    address = a.House.Address,
                })
                .ToListAsync();
            if (!currentAppartmentId.IsNullOrEmpty())
                ViewBag.CurrentApartmentId = Convert.ToInt32(currentAppartmentId);
            else ViewBag.CurrentApartmentId = apartments.FirstOrDefault().ApartmentId;
            return View(apartments);
        }
    }   
}
public class ApartmentSwitcherItem
{
    public int ApartmentId { get; set; }
    public string address { get; set; }
    public string Number { get; set; }
}

