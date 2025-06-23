using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels.User;

namespace TSZH_Komarov.Controllers
{
    
    public class UserController : Controller
    {
        private UserService userService;
        private ProfileService profileService;
        private TelegramService telegramService;
        private readonly ILogger<UserController> logger;
        public UserController(UserService userService, ProfileService profileService, ILogger<UserController> logger, TelegramService telegramService)
        {
            this.userService = userService;
            this.profileService = profileService;
            this.logger = logger;
            this.telegramService = telegramService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Registration()
        {
            var model = await userService.LoadDataForRegAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            TempData["ActiveTab"] = "personal";
            var model = profileService.GetUserData();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetCurrentTszh(int tszhId)
        { 
            var identity = (ClaimsIdentity)User.Identity;
            var existingClaim = identity.FindFirst("tszh");
            var existingName = identity.FindFirst("tszhName");
            var existingAppart = identity.FindFirst("appartment");

            if (existingClaim != null || existingName != null || existingAppart != null)
            {
                identity.RemoveClaim(existingClaim);
                identity.RemoveClaim(existingName);
                identity.RemoveClaim(existingAppart);
            }

            identity.AddClaim(new Claim("tszh", tszhId.ToString()));

            string tszhName = userService.GetCurrTszh().Name;
            int currUserId = userService.GetCurrUser().UserId;
            var appartList = userService.GetUserApartmentList(currUserId, tszhId);

            identity.AddClaim(new Claim("tszhName", tszhName));
            identity.AddClaim(new Claim("appartment", appartList[0].ApartmentId.ToString()));

            string authType = CookieAuthenticationDefaults.AuthenticationScheme;
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);


            await HttpContext.SignInAsync(authType, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SetCurrentApartment(int apartmentId)
        {
            var identity = (ClaimsIdentity)User.Identity;

            var existingClaim = identity.FindFirst("appartment");

            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }

            var apartment = userService.GetApartments(apartmentId);

            if (apartment == null)
            {
                return NotFound();
            }

            identity.AddClaim(new Claim("appartment", apartment.ApartmentId.ToString()));

            string authType = CookieAuthenticationDefaults.AuthenticationScheme;
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(authType, principal);
            return RedirectToAction("Services", "Services");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            TempData["ActiveTab"] = "personal";
            ModelState.Remove("OldPassword");
            ModelState.Remove("NewPassword");
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }
            if (await profileService.changeProfile(model))
            {
                TempData["Message"] = "Данные успешно изменены!";
                return View("Profile", model);
            }
            TempData["Message"] = "Произошла ошибка! Попробуйте еще раз!";
            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSecurity(ProfileViewModel model)
        {
            TempData["ActiveTab"] = "security";
            ModelState.Remove("FullName");
            ModelState.Remove("Email");
            ModelState.Remove("PhoneNumber");
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            string salt = userService.GetSalt();
            string hashedPass = userService.GetSha256(model.NewPassword, salt);

            if (model.OldPassword != model.OldPasswordFixed && userService.GetSha256(model.OldPassword, model.Salt) != model.OldPasswordFixed)
            {
                TempData["Message"] = "Произошла ошибка! Некорректный старый пароль!";
                return View("Profile", model);
            }
            if (await profileService.changeSecurity(hashedPass, salt))
            {
                TempData["Message"] = "Данные успешно изменены!";
                return View("Profile", model);
            }
            TempData["Message"] = "Произошла ошибка! Попробуйте еще раз!";
            return View("Profile", model);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }
            try
            {
                AppUser? user = await userService.LoginAsync(login.Email, login.Password);
                if (user == null)
                {
                    ModelState.AddModelError("log_error", $"Неверный email или пароль");
                    return View(login);
                }
                if (user.IsFirstLogin == 0)
                {
                    userService.SetFirstLogin(user.UserId);
                    await SignIn(user);
                    return RedirectToAction("Profile", "User");
                }
                await SignIn(user);
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError("log_error", $"Не удалось авторизоваться, попробуйте попытку авторизации позже");
                return View(login);
            }
        }

        private async Task SignIn(AppUser user)
        {
            string role = user.Role switch
            {
                0 => "client",
                1 => "admin",
                _ => throw new ApplicationException("invalid user role")
            };

            var tszhList = userService.GetUserTszhList(user.UserId);
            var appartList = userService.GetUserApartmentList(user.UserId, tszhList[0].TszhId);

            List<Claim> claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("id", user.UserId.ToString()),
                new Claim("role", role),
                new Claim("tszh", tszhList[0].TszhId.ToString()),
                new Claim("tszhName", tszhList[0].Name.ToString()),
                new Claim("appartment", appartList[0].ApartmentId.ToString()),
            };

            string authType = CookieAuthenticationDefaults.AuthenticationScheme;
            IIdentity identity = new ClaimsIdentity(claims, authType);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await userService.UpdateData(model);
                return View(model);
            }

            string msg = await userService.RegistrationAsync(model);
            TempData["Message"] = msg;


            return RedirectToAction("Registration");
        }

        [HttpGet]
        public async Task<IActionResult> GetApartments(int houseId)
        {
            var apartments = await userService.GetApartmentsByHouseId(houseId);
            return Json(apartments.Select(a => new {
                apartmentId = a.ApartmentId,
                number = a.Number
            }));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotifications(ProfileViewModel model)
        {
            TempData["ActiveTab"] = "notifications";

            string msg = await profileService.UpdateReminderDaysBefore(model.ReminderDaysBefore);

            TempData["Message"] = msg;
            return View("Profile", model);
        }

        [HttpGet("Profile/generate-telegram-link")]
        public IActionResult GenerateTelegramLink()
        {
            try
            {
                var userId = userService.GetCurrUser().UserId;
                var botLink = telegramService.GenerateBotLink(userId);
                return Json(new { link = botLink });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при генерации Telegram ссылки");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("Profile/check-telegram-binding")]
        public async Task<IActionResult> CheckTelegramBinding()
        {
            TempData["ActiveTab"] = "notifications";
            try
            {
                var user = userService.GetCurrUser();
                
                return Json(new
                {
                    isBound = !string.IsNullOrEmpty(user?.ChatId),
                    chatId = user?.ChatId
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка проверки привязки Telegram");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
