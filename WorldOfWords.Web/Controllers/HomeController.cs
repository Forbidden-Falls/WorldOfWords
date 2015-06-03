namespace WorldOfWords.Web.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Models;
    using ViewsModels;
    using WebGrease;

    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            this.BoardsManager.Execute();
            var homeInfo = new HomeViewModel();
            this.Session["boardName"] = null;


            //take all boards
            var boardsDb = this.Data.Boards
                .Select(b => b)
                .ToList();

            if (boardsDb.Count != 0)
            {
                homeInfo.AllBoards = new List<Board>();
                foreach (var board in boardsDb)
                {
                    homeInfo.AllBoards.Add(new Board
                    {
                        Name = board.Name,
                        ExpirationDate = board.ExpirationDate,
                        Size = board.Size,
                        Content = board.Content
                    });
                }
            }

            //take User info
            if (this.User.Identity.IsAuthenticated)
            {
                string userId = this.User.Identity.GetUserId();
                var userStats = this.Data.Users
                    .FirstOrDefault(u => u.Id == userId);

                homeInfo.UserName = User.Identity.Name;
                homeInfo.Balance = userStats.Balance;
                homeInfo.RegisteredOn = userStats.RegisteredOn;
                homeInfo.EarnedPoints = userStats.EarnedPoints;

                if (userStats.BoardsUsers != null)
                {
                    homeInfo.BoardsUsers = userStats.BoardsUsers.Select(x => new BoardsUsers
                    {
                        UserPoints = x.UserPoints,
                        Board = x.Board
                    })
                    .ToList();
                }

                if (userStats.WordsUsers != null)
                {
                    homeInfo.WordsUsers = userStats.WordsUsers.Select(x => new WordsUsers()
                    {
                        Word = x.Word,
                        WordCount = x.WordCount
                    })
                    .OrderBy(x => x.Word.Content)
                    .GroupBy(x => x.Word.Content[0])
                    .ToList();
                }
            }

            return View(homeInfo);
        }


        public ActionResult Rating()
        {
            var loggedUser = new User();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                loggedUser = this.Data.Users.FirstOrDefault(u => u.Id == userId);
            }

            var users = this.Data.Users;
                //.OrderByDescending(u => u.Balance);

            if (users.Count() < 0)
            {
                return View();
            }

            var usersStatsViewModel = users
                .Where(u => u.Statistics != null)
                //.Take(25)
                .Select(u =>
                    new UserStatisticsViewModel
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Balance = u.Balance,
                        MostPointsOfWord = u.Statistics == null ? 0 : u.Statistics.MostPointsOfWord,
                        SpentMoney = u.Statistics == null ? 0 : u.Statistics.SpentMoney,
                        EarnedPoints = u.EarnedPoints,
                        Email = u.Email
                    });

            var usersStatsBalanceOrdered = usersStatsViewModel
                .OrderByDescending(u => u.Balance)
                .Take(25)
                .ToList();

            var usersStatsPointsOrdered = usersStatsViewModel
                .OrderByDescending(x => x.EarnedPoints)
                .Take(25)
                .ToList();

            var usersStatsSpendMoneyOrdered = usersStatsViewModel
                .OrderByDescending(x => x.SpentMoney)
                .Take(25)
                .ToList();

            var usersStatsMostPointsOfWord = usersStatsViewModel
                .OrderByDescending(x => x.MostPointsOfWord)
                .Take(25)
               .ToList();

            var raitingViewModel = new RaitingViewModel
                {
                    LoggedUser = loggedUser,
                    UsersStatsBalance = usersStatsBalanceOrdered,
                    UsersStatsPoints = usersStatsPointsOrdered,
                    UsersStatsSpendMoney = usersStatsSpendMoneyOrdered,
                    UsersStatsMostPointOfWord = usersStatsMostPointsOfWord,

                };

            // If have logged user(Aways TRUE for now), fill this prop with user place in the Rang Lists.
            // Work With Query list downloaded once from context.
            if (loggedUser.Id != null)
            {
                raitingViewModel.UserPlaceBalance = users
                    .OrderByDescending(u => u.Balance)
                    .ToList()
                    .FindIndex(u => u.Id == loggedUser.Id);
                raitingViewModel.UserPlaceBalance++;

                raitingViewModel.UserPlacePoints = users
                    .OrderByDescending(u => u.EarnedPoints)
                    .ToList()
                    .FindIndex(u => u.Id == loggedUser.Id);
                raitingViewModel.UserPlacePoints++;

                raitingViewModel.UserPlaceSpendMoney = users
                    .Where(u => u.Statistics != null)
                    .OrderByDescending(u => u.Statistics.SpentMoney)
                    .ToList()
                    .FindIndex(u => u.Id == loggedUser.Id);
                raitingViewModel.UserPlaceSpendMoney++;

                raitingViewModel.UserPlaceMostPointOfWord = users
                    .Where(u => u.Statistics != null)
                    .OrderByDescending(u => u.Statistics.MostPointsOfWord)
                    .ToList()
                    .FindIndex(u => u.Id == loggedUser.Id);
                raitingViewModel.UserPlaceMostPointOfWord++;
            }

            return View(raitingViewModel);
        }

        public ActionResult Rules()
        {
            return View();

        }
    }
}