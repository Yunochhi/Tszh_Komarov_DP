using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TSZH_Komarov.Services
{
    public class VotingService
    {
        private Data.TszhKomarovContext context;
        private UserService userService;
        AppUser currUser;

        public VotingService(Data.TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;

            currUser = userService.GetCurrUser();
        }

        public List<Poll> GetPolls(string voteType)
        {
            var currtszh = userService.GetCurrTszh().TszhId;

            var activePolls = context.Polls
            .Include(p => p.VoteType)
            .Include(p => p.PollOptions)
            .ThenInclude(po => po.Votes)
            .Where(p => p.EndDate > DateTime.Now).Where(p => p.TszhId == currtszh);

            if (!string.IsNullOrEmpty(voteType))
            {
                activePolls = activePolls.Where(p => p.VoteType.Name == voteType);
            }
            if (voteType == "Завершенные")
            {
                var nonActivePolls = context.Polls
                .Include(p => p.VoteType)
                .Include(p => p.PollOptions)
                .ThenInclude(po => po.Votes)
                .Where(p => p.EndDate < DateTime.Now).Where(p => p.TszhId == currtszh);
                return nonActivePolls.ToList();
            }

            return activePolls.ToList();
        }
        public bool AddPollOption(int pollOptionId)
        {
            var pollOption = context.PollOptions
            .Include(po => po.Poll)
            .FirstOrDefault(po => po.PollOptionId == pollOptionId);       
           
            try{
                context.Votes.Add(new Vote
                {
                    PollOptionId = pollOptionId,
                    UserId = currUser.UserId
                });
                context.SaveChangesAsync();
                return true;
            }
            catch {
                return false;
            }        
        }
    }

}
