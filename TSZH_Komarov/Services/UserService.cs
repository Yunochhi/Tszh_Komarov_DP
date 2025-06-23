using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
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
            
            try
            {
                AppUser? toAdd = null;
                if (await context.AppUsers.AnyAsync(c => c.Email == model.Email || c.PhoneNumber == model.PhoneNumber))
                {
                    toAdd = await context.AppUsers.FirstOrDefaultAsync(c => c.Email == model.Email);
                }

                if (toAdd == null)
                {
                    toAdd = new AppUser
                    {
                        Fullname = model.Fullname,
                        Email = model.Email,
                        Salt = GetSalt(),
                        Role = 0,
                        PhoneNumber = model.PhoneNumber,
                        ReminderDaysBefore = 3,
                        IsFirstLogin = 0,
                    };
                    toAdd.Password = GetSha256(model.Password, toAdd.Salt);

                    await context.AppUsers.AddAsync(toAdd);
                    await context.SaveChangesAsync();
                }

                var apartment = await context.Apartments.FindAsync(model.SelectedApartmentId);
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

        public Apartment GetCurrAppartment()
        {
            var claimsUser = httpContextAccessor.HttpContext.User;
            int id = Convert.ToInt32(claimsUser.FindFirst("appartment")?.Value);
            var apart = context.Apartments.Include(h => h.House).Where(c => c.ApartmentId == id)
                .FirstOrDefault();
            return apart;
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

        public Apartment GetApartments(int apartmentId)
        {
            var user = GetCurrUser();
            var apartment = context.Apartments
                .FirstOrDefault(a => a.ApartmentId == apartmentId && a.UserId == user.UserId);

            return apartment;
        }

        public List<Apartment> GetUserApartmentList(int userId, int currentTszhId)
        {
            var apartments = context.Apartments
                .Include(h => h.House)
                .Where(a => a.UserId == userId && a.House.TszhId == currentTszhId)
                .ToList();

            return apartments;
        }
    }
}
