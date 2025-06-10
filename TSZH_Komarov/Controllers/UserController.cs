using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using Microsoft.AspNetCore.Identity;
using TSZH_Komarov.Viewmodels.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;

namespace TSZH_Komarov.Controllers
{
    
    public class UserController : Controller
    {
        private UserService userService;
        private ProfileService profileService;
        private readonly ILogger<UserController> logger;
        public UserController(UserService userService, ProfileService profileService, ILogger<UserController> logger   )
        {
            this.userService = userService;
            this.profileService = profileService;
            this.logger = logger;
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
            // Обновление claim
            var identity = (ClaimsIdentity)User.Identity;
            var existingClaim = identity.FindFirst("tszh");
            var existingName = identity.FindFirst("tszhName");

            if (existingClaim != null || existingName != null)
            {
                identity.RemoveClaim(existingClaim);
                identity.RemoveClaim(existingName);
            }

            identity.AddClaim(new Claim("tszh", tszhId.ToString()));
            string tszhName = userService.GetCurrTszh().Name;
            identity.AddClaim(new Claim("tszhName", tszhName));

            string authType = CookieAuthenticationDefaults.AuthenticationScheme;
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);


            await HttpContext.SignInAsync(authType, principal);

            return RedirectToAction("Index", "Home");
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
            if (await profileService.changeSecurity(hashedPass, salt))
            {
                TempData["Message"] = "Данные успешно изменены!";
                return View("Profile", model);
            }
            TempData["Message"] = "Произошла ошибка! Попробуйте еще раз!";
            return View("Profile", model);
        }

        /*[HttpPost]
        public async Task<IActionResult> UpdateNotifications(ProfileViewModel model)
        {
            TempData["ActiveTab"] = "notifications";

            ModelState.Remove("FullName");
            ModelState.Remove("Email");
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("OldPassword");
            ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            if (await profileService.changeNotification(model.NotificationMethod))
            {
                TempData["Message"] = "Данные успешно изменены!";
                return View("Profile", model);
            }
            TempData["Message"] = "Произошла ошибка! Попробуйте еще раз!";
            return View("Profile", model);
        }*/

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

            List<Claim> claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("id", user.UserId.ToString()),
                new Claim("role", role),
                new Claim("tszh", tszhList[0].TszhId.ToString()),
                new Claim("tszhName", tszhList[0].Name.ToString()),
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

    }
}
