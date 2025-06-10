using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels.User;

namespace TSZH_Komarov.Services
{
    public class ProfileService
    {
        private Data.TszhKomarovContext context;
        private AppUser user;
        private UserService userService;
        public ProfileService(Data.TszhKomarovContext context, IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            this.context = context;

            var claimsUser = httpContextAccessor.HttpContext.User;
            int id = Convert.ToInt32(claimsUser.FindFirst("id")?.Value);

            user = context.AppUsers.Include(c => c.Apartments).ThenInclude(c => c.House).Where(c => c.UserId == id).FirstOrDefault();  
        }

        public ProfileViewModel GetUserData()
        {
            var model = new ProfileViewModel()
            {
                FullName = user.Fullname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PersonalAccount = user.Apartments.First().PersonalAccount,
                OldPassword = user.Password,
                IsFirtsLogin = user.IsFirstLogin
            };
            return model;
        }

        public async Task<bool> changeProfile(ProfileViewModel model)
        {
            try
            {
                var changeUser = context.AppUsers.Where(c => c.UserId == user.UserId).FirstOrDefault();
                changeUser.Fullname = model.FullName;
                changeUser.PhoneNumber = model.PhoneNumber;
                changeUser.Email = model.Email;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> changeSecurity(string hashPass, string salt)
        {
            try
            {
                var changeUser = context.AppUsers.Where(c => c.UserId == user.UserId).FirstOrDefault();

                changeUser.Password = hashPass;
                changeUser.Salt = salt;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
      
    
    }
}
