namespace WorldOfWords.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;
    using BindingModels;
    using Common;
    using Data;
    using Data.Contracts;
    using Data.Migrations;

    using Models;

    public abstract class BaseController : Controller
    {
        protected BaseController()
        {
            this.RandomGenerator = new Random();
            var context = new WorldOfWordsDbContext();
            this.Data = new WorldOfWordsData(context);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<WorldOfWordsDbContext, Configuration>());
            
            this.WordAssessor = new Assessor(Config.Language, this.Data);

            if (this.Data.Boards.Count() < 3)
            {
                CreateBoard();               
            }

            AddUsersPointsFromClosedBoards();
            DeleteClosedBoards();
        }

        private Random RandomGenerator { get; set; }

        protected IWorldOfWordsData Data { get; set; }

        protected Assessor WordAssessor { get; set; }

        private BoardName GetRondomBoardName()
        {

            var boardName = this.Data.BoardName
                .Where(bn => bn.Board == null)
                .OrderBy(r => Guid.NewGuid())
                .Take(1)
                .FirstOrDefault();

            return boardName;
        }

        private void CreateBoard()
        {
            var randomSize = this.RandomGenerator.Next(Config.MinSize, Config.MaxSize + 1);
            var randomDuration = this.RandomGenerator.Next(Config.MinDurationInMinutes, Config.MaxDurationInMinutes + 1);
            var boardName = this.GetRondomBoardName();

            if (boardName == null)
            {
                return;
            }

            boardName.Board = new Board
            {
                Name = boardName,
                Size = randomSize,
                ExpirationDate = DateTime.Now.AddMinutes(randomDuration),
                Content = new string(' ', randomSize * randomSize)
            };

            this.Data.SaveChanges();
        }

        private void AddUsersPointsFromClosedBoards()
        {
            var users = this.Data.BoardsUsers
                .Where(bu => bu.Board.ExpirationDate < DateTime.Now)
                .GroupBy(bu => bu.UserId)
                .Select(g => new UserModel
                {
                    Id = g.Key,
                    Points = g.Sum(u => u.UserPoints)
                })
                .ToList();

            foreach (var user in users)
            {
                var currentUser = this.Data.BoardsUsers.First(u => u.UserId == user.Id);
                currentUser.User.Balance += user.Points;
                currentUser.User.EarnedPoints += user.Points;

                this.Data.SaveChanges();
            }
        }

        private void DeleteClosedBoards()
        {
            var boardsUsers = this.Data.BoardsUsers
                .Where(bu => bu.Board.ExpirationDate < DateTime.Now);

            foreach (var bu in boardsUsers)
            {
                this.Data.BoardsUsers.Delete(bu);
            }

            var boardsForDeleting = this.Data.Boards
                .Where(b => b.ExpirationDate < DateTime.Now);

            foreach (var board in boardsForDeleting)
            {
                this.Data.Boards.Delete(board);
            }

            this.Data.SaveChanges();

        }
    }
}