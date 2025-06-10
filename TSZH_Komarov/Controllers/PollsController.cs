using Microsoft.AspNetCore.Mvc;
using TSZH_Komarov.Services;

namespace TSZH_Komarov.Controllers
{
    public class PollsController : Controller
    {
        private VotingService votingService;
        string voteType;

        public PollsController(VotingService votingService)
        {
            this.votingService = votingService;
        }

        public IActionResult Voting(string voteType)
        {      
            var activePolls = votingService.GetPolls(voteType);
            ViewBag.ActiveFilter = voteType;
            this.voteType = voteType;
            return View(activePolls);
        }

        [HttpPost]
        public async Task<IActionResult> Voting(int PollOptionId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Voting", "Polls");
            }
            if (votingService.AddPollOption(PollOptionId))
            {
                return RedirectToAction($"Voting", "Polls");
            }
            return RedirectToAction("Voting", "Polls");
        }
    }
}
