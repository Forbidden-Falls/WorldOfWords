namespace WorldOfWords.Web.Common
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BindingModels;
    using Data.Contracts;
    using Models;

    public class BoardsManager
    {
        public BoardsManager(IWorldOfWordsData data)
        {
            this.RandomGenerator = new Random();
            this.Data = data;
        }

        private IWorldOfWordsData Data { get; set; }

        private Random RandomGenerator { get; set; }

        public void Execute()
        {
            AddUsersPointsFromClosedBoards();
            DeleteClosedBoards();
            var missingBoards = Config.MaxNumberOfEmptyBoards - this.GetCountOfEmptyBoards();
            for (int i = 0; i < missingBoards; i++)
            {
                CreateBoard();
            }
        }

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

        private int GetCountOfEmptyBoards()
        {
            var count = this.Data.Boards
                .ToList()
                .Count(b => Assessor.GetPercentOfFilling(b.Content) < Config.FillingPercent);
            return count;
        }
    }
}