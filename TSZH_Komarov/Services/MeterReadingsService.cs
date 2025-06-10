using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels.Readings;

namespace TSZH_Komarov.Services
{
    public class MeterReadingsService
    {
        private Data.TszhKomarovContext context;
        private UserService userService;

        private Tszh currTszh;
        private AppUser currUser;

        public MeterReadingsService(Data.TszhKomarovContext context, IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            this.context = context;
            this.userService = userService;

            currTszh = userService.GetCurrTszh();
            currUser = userService.GetCurrUser();
        }
        public List<ApartmentMetersViewModel> GetMeters()
        {
            return currUser.Apartments.Where(c => c.House.TszhId == currTszh.TszhId)
                .Select(apartment => new ApartmentMetersViewModel
                {
                    ApartmentId = apartment.ApartmentId,
                    AccountNumber = apartment.PersonalAccount,
                    Meters = context.MeterTypes
                        .Select(mt => new MeterReadingViewModel
                        {
                            ApartmentId = apartment.ApartmentId,
                            MeterTypeId = mt.MeterTypeId,
                            MeterName = mt.Name,
                            Unit = mt.Unit,
                            PreviousValue = context.MeterReadings
                                .Where(mr => mr.MeterTypeId == mt.MeterTypeId
                                          && mr.ApartamentId == apartment.ApartmentId)
                                .OrderByDescending(mr => mr.ReadingDate)
                                .Select(mr => mr.Value)
                                .FirstOrDefault()
                        }).ToList()
                }).ToList();
        }

        public List<MeterReadingHistViewModel> GetMeterHistory(int apartmentId)
        {
            if (!currUser.Apartments.Any(a => a.ApartmentId == apartmentId))
                return new List<MeterReadingHistViewModel>();

            var allReadings = context.MeterReadings
                .Where(mr => mr.ApartamentId == apartmentId)
                .Include(mr => mr.MeterType)
                .ToList();

            var allMeterTypes = context.MeterTypes.ToList();

            var history = allReadings
                .GroupBy(mr => mr.ReadingDate.ToString("yyyyMMddHHmmss"))
                .Select(g => new MeterReadingHistViewModel
                {
                    ReadingDate = DateTime.ParseExact(g.Key, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
                    readingHistItems = allMeterTypes.Select(mt =>
                    {
                        var reading = g.FirstOrDefault(r => r.MeterTypeId == mt.MeterTypeId);
                        return new ReadingHistItem
                        {
                            MeterName = mt.Name,
                            Value = reading != null ? reading.Value : 0,
                            Unit = mt.Unit,
                        };
                    }).ToList()
                })
                .OrderByDescending(h => h.ReadingDate)
                .ToList();

            return history;
        }

        public async Task<bool> AddMetersForApartment(int apartmentId, List<MeterReadingViewModel> models)
        {
            try
            {
                var userApartmentIds = await context.Apartments
                    .Where(a => a.UserId == currUser.UserId)
                    .Select(a => a.ApartmentId)
                    .ToListAsync();

                if (!userApartmentIds.Contains(apartmentId))
                    return false;

                foreach (var model in models)
                {
                    var previousValue = await context.MeterReadings
                        .Where(mr => mr.MeterTypeId == model.MeterTypeId
                                  && mr.ApartamentId == apartmentId)
                        .OrderByDescending(mr => mr.ReadingDate)
                        .Select(mr => mr.Value)
                        .FirstOrDefaultAsync();

                    if (model.CurrentValue < previousValue)
                    {
                        return false;
                    }
                }

                // Сохранение показаний
                var readings = models.Select(r => new MeterReading
                {
                    ApartamentId = apartmentId,
                    MeterTypeId = r.MeterTypeId,
                    Value = (double)r.CurrentValue,
                    ReadingDate = DateTime.Now
                });

                await context.MeterReadings.AddRangeAsync(readings);
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
