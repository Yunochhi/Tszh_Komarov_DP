using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TSZH_Komarov.Data;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels;

public class TszhSwitcherViewComponent : ViewComponent
{
    private readonly TszhKomarovContext context;
    private readonly UserService userService;

    public TszhSwitcherViewComponent(TszhKomarovContext context, UserService userService)
    {
        this.context = context;
        this.userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = userService.GetCurrUser().UserId;
        var currentTszhId = UserClaimsPrincipal.FindFirstValue("tszh");


        var tszhList = await context.Apartments
            .Where(a => a.UserId == userId)
            .Select(a => new
            {
                TszhId = a.House.Tszh.TszhId,
                Name = a.House.Tszh.Name,
                Address = a.House.Address
            })
            .Distinct()
            .ToListAsync();

        // Преобразуем в строго типизированный список
        var result = tszhList.Select(t => new TszhSwitcherItem
        {
            TszhId = t.TszhId,
            Name = t.Name,
            Address = t.Address
        }).ToList();

        ViewBag.CurrentTszhId = currentTszhId;
        return View(result);
    }
}