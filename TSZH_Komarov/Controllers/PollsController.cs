using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels;

namespace TSZH_Komarov.Controllers
{
    public class PollsController : Controller
    {
        private VotingService votingService;
        private readonly ILogger<HomeController> logger;
        string voteType;

        public PollsController(VotingService votingService, ILogger<HomeController> logger)
        {
            this.votingService = votingService;
            this.logger = logger;
        }

        public IActionResult Voting(string? voteType)
        {
            if (TempData["ActiveFilter"] != null && string.IsNullOrEmpty(voteType))
            {
                voteType = TempData["ActiveFilter"] as string;
                TempData.Keep("ActiveFilter"); // Сохраняем для следующего запроса
            }

            var activePolls = votingService.GetPolls(voteType);
            ViewBag.ActiveFilter = voteType;
            ViewBag.VotingService = votingService;
            return View(activePolls);
        }

        public IActionResult CreatePoll()
        {
            var model = new PollCreateViewModel
            {
                VoteTypes = votingService.loadVoteTypes()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Voting(int pollOptionId, string? voteType)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var error = ModelState[key].Errors.FirstOrDefault();
                    if (error != null)
                    {
                        logger.LogError($"Ошибка в поле '{key}': {error.ErrorMessage}");
                    }
                }
                return RedirectToAction("Voting", "Polls", new { voteType });
            }
            TempData["ActiveFilter"] = voteType;
            if (votingService.AddPollOption(pollOptionId))
            {
                return RedirectToAction("Voting", "Polls", new { voteType });
            }

            return RedirectToAction("Voting", "Polls", new { voteType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePoll(PollCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var error = ModelState[key].Errors.FirstOrDefault();
                    if (error != null)
                    {
                        logger.LogError($"Ошибка в поле '{key}': {error.ErrorMessage}");
                    }
                }
                model.VoteTypes = votingService.loadVoteTypes();
                return View(model);
            }

            string msg = await votingService.addNewVote(model);
            TempData["Message"] = msg;
            return RedirectToAction("CreatePoll", "Polls");
        }
    }
}
