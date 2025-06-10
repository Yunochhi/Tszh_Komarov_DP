using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels;
using TSZH_Komarov.Viewmodels.User;

namespace TSZH_Komarov.Services
{
    public class UserService
    {
        IHttpContextAccessor httpContextAccessor;
        private Data.TszhKomarovContext context;
        public UserService(Data.TszhKomarovContext context, IHttpContextAccessor httpContextAccessor) 
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }
        public string GetSalt() => DateTime.UtcNow.ToString() + DateTime.Now.Ticks;

        public string GetSha256(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = SHA256.HashData(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }
        public async Task<AppUser?> LoginAsync(string email, string password)
        {
            var user = await context.AppUsers.FirstOrDefaultAsync(c => c.Email == email);

            if (user is null || user.Password != GetSha256(password, user.Salt))
            {
                return null; 
            }
    
            return user;
        }

        public async Task<string> RegistrationAsync(RegistrationViewModel model)
        { 
            if (await context.AppUsers.AnyAsync(c => c.Email == model.Email || c.PhoneNumber == model.PhoneNumber))
            {
                return "Введенные почта или телефон уже существуют в системе!";
            }
            try
            {
                AppUser toAdd = new AppUser
                {
                    Fullname = model.Fullname,
                    Email = model.Email,
                    Salt = GetSalt(),
                    Role = 0,
                    PhoneNumber = model.PhoneNumber
                };
                toAdd.Password = GetSha256(model.Password, toAdd.Salt);

                var apartment = await context.Apartments.FindAsync(model.SelectedApartmentId);
                if (apartment == null)
                {
                    return "при выборе квартиры произошла ошибка!";
                }
                await context.AppUsers.AddAsync(toAdd);
                await context.SaveChangesAsync();

                apartment.UserId = toAdd.UserId;
                context.Update(apartment);
                await context.SaveChangesAsync();

                return "Пользователь успешно зарегистрирован!";
            }
            catch
            {
                return "При регистрации произошла ошибка!";
            }       
        }

        public AppUser GetCurrUser()
        {
            var claimsUser = httpContextAccessor.HttpContext.User;
            int id = Convert.ToInt32(claimsUser.FindFirst("id")?.Value);
            var user = context.AppUsers.Include(c => c.Apartments)
                .ThenInclude(c => c.House).ThenInclude(c => c.Tszh).Where(c => c.UserId == id)
                .FirstOrDefault();
            return user;
        }

        public Tszh GetCurrTszh()
        {
            var claimsUser = httpContextAccessor.HttpContext.User;
            int id = Convert.ToInt32(claimsUser.FindFirstValue("tszh"));
            var tszh = context.Tszhs.Find(id);
            return tszh;
        }

        public List<TszhSwitcherItem> GetUserTszhList(int userId)
        {
            var apartments = context.Apartments
               .Include(a => a.House)
               .ThenInclude(h => h.Tszh)
               .Where(a => a.UserId == userId)
               .ToList();

            var tszhList = new List<TszhSwitcherItem>();

            foreach (var apartment in apartments)
            {
                if (apartment.House?.Tszh != null)
                {
                    var tszh = apartment.House.Tszh;
                    if (!tszhList.Any(t => t.TszhId == tszh.TszhId))
                    {
                        tszhList.Add(new TszhSwitcherItem
                        {
                            TszhId = tszh.TszhId,
                            Name = tszh.Name,
                            Address = apartment.House.Address
                        });
                    }
                }
            }

            return tszhList;

        }

        public void SetFirstLogin(int userId)
        {
            var user = context.AppUsers.Where(c => c.UserId == userId).FirstOrDefault();

            user.IsFirstLogin = 1;
            context.SaveChanges();
        }

        public async Task<RegistrationViewModel> LoadDataForRegAsync()
        {
            int currentTszhId = GetCurrTszh().TszhId;

            return new RegistrationViewModel
            {
                CurrentTszhId = currentTszhId,
                Houses = await context.Houses
                    .Where(h => h.TszhId == currentTszhId)
                    .Select(h => new HousesItem
                    {
                        HouseId = h.HouseId,
                        Address = h.Address
                    })
                    .ToListAsync()
            };
        }

        public async Task<RegistrationViewModel> UpdateData(RegistrationViewModel model)
        {
            model.Houses = await context.Houses
                .Where(h => h.TszhId == model.CurrentTszhId)
                .Select(h => new HousesItem
                {
                    HouseId = h.HouseId,
                    Address = h.Address
                })
                .ToListAsync();

            // Всегда загружаем квартиры если выбран дом
            if (model.SelectedHouseId > 0)
            {
                model.Apartments = await GetApartmentsByHouseId(model.SelectedHouseId);
            }

            return model;
        }

        public async Task<List<ApartmentsItem>> GetApartmentsByHouseId(int houseId)
        {
            return await context.Apartments
                .Where(a => a.HouseId == houseId)
                .Select(a => new ApartmentsItem
                {
                    ApartmentId = a.ApartmentId,
                    Number = a.Number
                })
                .ToListAsync();
        }
    }
}
