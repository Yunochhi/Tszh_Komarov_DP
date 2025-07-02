using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels;
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
            IQueryable<Poll> query = context.Polls
                .Include(p => p.VoteType)
                .Include(p => p.PollOptions)
                .ThenInclude(po => po.Votes)
                .Where(p => p.TszhId == currtszh);

            if (string.Equals(voteType, "Завершенные", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.EndDate < DateTime.Now);
            }
            else
            {
                query = query.Where(p => p.EndDate > DateTime.Now);

                if (!string.IsNullOrEmpty(voteType))
                {
                    query = query.Where(p => p.VoteType.Name == voteType);
                }
            }


            return query.ToList();
        }
        public bool AddPollOption(int pollOptionId)
        {
            var pollOption = context.PollOptions
                .Include(po => po.Poll)
                .FirstOrDefault(po => po.PollOptionId == pollOptionId);

            if (pollOption == null) return false;

            var currentTszh = userService.GetCurrTszh();
            if (currentTszh == null) return false;

            // Вычисляем общую площадь квартир пользователя в текущем ТСЖ
            var userApartments = context.Apartments
                .Include(a => a.House)
                .Where(a => a.UserId == currUser.UserId &&
                           a.House.TszhId == currentTszh.TszhId)
                .ToList();

            double totalSquare = userApartments.Sum(a => a.Meters);

            try
            {
                context.Votes.Add(new Vote
                {
                    PollOptionId = pollOptionId,
                    UserId = currUser.UserId,
                    Square = totalSquare
                });
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public (PollOption? Winner, double TotalSquare) GetPollWinner(Poll poll)
        {
            var allVotes = poll.PollOptions.SelectMany(o => o.Votes).ToList();
            double totalSquare = allVotes.Sum(v => v.Square);

            // Находим вариант с максимальной суммой площадей
            var winner = poll.PollOptions
                .OrderByDescending(o => o.Votes.Sum(v => v.Square))
                .FirstOrDefault();

            return (winner, totalSquare);
        }
        public SelectList loadVoteTypes()
        {
            var selectList = new SelectList(context.VoteTypes, "VoteTypeId", "Name");
           
            return selectList;
        }

        public async Task<string> addNewVote(PollCreateViewModel model)
        {
            var validOptions = model.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            if (validOptions.Count < 3)
            {
                return "Добавьте минимум 3 варианта ответа";
            }

            var currentTszh = userService.GetCurrTszh();

            var newPoll = new Poll
            {
                Title = model.Title,  
                VoteTypeId = model.VoteTypeId,
                EndDate = model.EndDate,
                TszhId = currentTszh.TszhId
            };

            // Добавляем варианты ответов
            foreach (var optionText in validOptions)
            {
                newPoll.PollOptions.Add(new PollOption { OptionText = optionText });
            }
            try
            {
                await context.Polls.AddAsync(newPoll);
                await context.SaveChangesAsync();
                return "Голосование успешно добавлено!";
            }
            catch
            {
                return "При добавлении голосования произошла ошибка!";
            }
           
        }
    }

}
