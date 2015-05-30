﻿namespace WorldOfWords.Web.Controllers
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
            var LoggedUser = new User();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                LoggedUser = this.Data.Users.FirstOrDefault(u => u.Id == userId);
            }

            var queryOrderedByBalance = this.Data.Users.Include(u=>u.Statistics).OrderByDescending(u => u.Balance).ToList();

            var usersStatsBalanceOrdered = queryOrderedByBalance
                .Take(100)
                .Select(u =>
                    new User
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Balance = u.Balance,
                        Statistics = new Statistics
                        {
                            MostPointsOfWord = u.Statistics.MostPointsOfWord,
                            SpentMoney = u.Statistics.SpentMoney
                        },
                        EarnedPoints = u.EarnedPoints
                    })
                .ToList();

            var usersStatsPointsOrdered = usersStatsBalanceOrdered.OrderByDescending(x => x.EarnedPoints)
                .ToList();

            var usersStatsSpendMoneyOrdered = usersStatsBalanceOrdered.OrderByDescending(x => x.Statistics.SpentMoney)
                .ToList();

            var usersStatsMostPointsOfWord = usersStatsBalanceOrdered.OrderByDescending(x => x.Statistics.SpentMoney)
               .ToList();

            var raitingViewModel = new RaitingViewModel
                {
                    LoggedUser = LoggedUser,
                    UsersStatsBalance = usersStatsBalanceOrdered,
                    UsersStatsPoints = usersStatsPointsOrdered,
                    UsersStatsSpendMoney = usersStatsSpendMoneyOrdered,
                    UsersStatsMostPointOfWord = usersStatsMostPointsOfWord,

                };

            // If have logged user(Aways TRUE for now), fill this prop with user place in the Rang Lists.
            // Work With Query list downloaded once from context.
            if (LoggedUser.Id != null)
            {
                raitingViewModel.UserPlaceBalance = queryOrderedByBalance
                    .OrderByDescending(u => u.Balance)
                    .ToList()
                    .FindIndex(u => u.Id == LoggedUser.Id);
                raitingViewModel.UserPlaceBalance++;

                raitingViewModel.UserPlacePoints = queryOrderedByBalance
                    .OrderByDescending(u => u.EarnedPoints)
                    .ToList()
                    .FindIndex(u => u.Id == LoggedUser.Id);
                raitingViewModel.UserPlacePoints++;

                raitingViewModel.UserPlaceSpendMoney = queryOrderedByBalance
                    .OrderByDescending(u => u.Statistics.SpentMoney)
                    .ToList()
                    .FindIndex(u => u.Id == LoggedUser.Id);
                raitingViewModel.UserPlaceSpendMoney++;

                raitingViewModel.UserPlaceMostPointOfWord = queryOrderedByBalance
                    .OrderByDescending(u => u.Statistics.MostPointsOfWord)
                    .ToList()
                    .FindIndex(u => u.Id == LoggedUser.Id);
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