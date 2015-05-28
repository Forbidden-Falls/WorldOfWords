namespace WorldOfWords.Web.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Models;
    using ViewsModels;

    public class BoardsController : BaseController
    {
        // GET: Board by name
        [HttpGet]
        [Authorize]
        public ActionResult Show(string name)
        {
            var userId = this.User.Identity.GetUserId();
            var user = this.Data.Users
                .Include(u => u.WordsUsers)
                .Include("WordsUsers.Words")
                .First(u => u.Id == userId);

            var board = this.Data.Boards.FirstOrDefault(b => b.Name.Text == name);
            if (board == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            this.Session["boardName"] = board.Name.Text;

            if (board.Content == "")
            {
                board.Content = new String(' ', board.Size * board.Size);
                this.Data.SaveChanges();
            }

            var userWords = user.WordsUsers
                .Where(wu => wu.Word.Boards.All(b => b.Name.Text != name))
                .Select(wu => wu.Word.Content)
                .OrderBy(w => w.Length)
                .ThenBy(w => w)
                .ToList();

            var wordsPoints = userWords
                .ToList()
                .Select(this.WordAssessor.GetPointsByWord)
                .ToList();

            var boardUser = board.BoardsUsers.FirstOrDefault(bu => bu.UserId == userId);
            var userPointsFromBoard = boardUser == null ? 0 : boardUser.UserPoints;

            var showBoardModel = new ShowBoardModel
            {
                Board = board,
                UserWords = userWords,
                WordsPoints = wordsPoints,
                UserPointsFromBoard = userPointsFromBoard,
                EarnedUserPoints = user.EarnedPoints,
                UserBalance = user.Balance
            };

            return View(showBoardModel);
        }
    }
}