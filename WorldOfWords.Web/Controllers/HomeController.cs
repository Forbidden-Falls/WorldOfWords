namespace WorldOfWords.Web.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Models;
    using ViewsModels;

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
            var query = this.Data.Users.OrderByDescending(u => u.Balance)
                .ToList();

            var usersStatsBalanceOrdered = query.Select(u => 
                new User
                {
                    UserName = u.UserName,
                    Balance = u.Balance,
                    EarnedPoints = u.EarnedPoints
                })
                .ToList();

            var usersStatsPointsOrdered = usersStatsBalanceOrdered.OrderByDescending(x => x.EarnedPoints)
                .Take(100)
                .ToList();

            var LoggedUser = new User();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                LoggedUser = this.Data.Users.FirstOrDefault(u => u.Id == userId);
            }

            var model = new RaitingViewModel
                {
                    LoggedUser = LoggedUser,
                    UsersStatsBalance = usersStatsBalanceOrdered,
                    UsersStatsPoints = usersStatsPointsOrdered
                };

            return View(model);
        }

        public ActionResult Rules()
        {
            return View();
        }
    }

    public class RaitingViewModel
    {
        public List<User> UsersStatsBalance { get; set; }
        public List<User> UsersStatsPoints { get; set; }
        public User LoggedUser { get; set; }
    }
}