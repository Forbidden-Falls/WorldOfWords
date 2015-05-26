namespace WorldOfWords.Web.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using Controllers;
    using Data.Contracts;
    using Microsoft.AspNet.Identity;
    using Models;
    using Newtonsoft.Json;

    public class BoardsHub : BaseHub
    {
        private string BoardContent { get; set; }

        private IList<Char> CrossLetters { get; set; }

        public void JoinBoard(string name)
        {
            this.Groups.Add(this.Context.ConnectionId, name);
        }

        public object AddWordToBoard(string boardName, string addedWord, int catchedLetterId, bool isVertical, int dropCellId)
        {
            Board board = null;
            int userPoits = 0;
            int pointsOfWord = 0;
            var isBoardClosed = false;
            try
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    throw new AccessViolationException("User is not logged!");
                }

                var userId = Context.User.Identity.GetUserId();

                board = Data.Boards.FirstOrDefault(b => b.Name.Text == boardName);
                if (board == null)
                {
                    throw new ApplicationException(String.Format("Board with name {0} is not found.", boardName));
                }

                if (board.ExpirationDate < DateTime.Now)
                {
                    isBoardClosed = true;
                    throw new ApplicationException("The board is closed.");
                }

                var word = this.Data.WordsUsers
                    .Where(wu => wu.UserId == userId)
                    .FirstOrDefault(w => w.Word.Content == addedWord);
                if (word == null)
                {
                    throw new ApplicationException("User has no such word.");
                }

                var startPosition = this.GetStartPosition(board.Size, addedWord.Count(), catchedLetterId, isVertical,
                    dropCellId);

                if (startPosition < 0)
                {
                    throw new ApplicationException("The word is out board.");
                }

                this.AddWordInBoardContent(board, addedWord, catchedLetterId, isVertical, dropCellId, startPosition);
                var newContent = this.BoardContent;

                if (newContent == null)
                {
                    throw new ApplicationException("Illegal crossing of words.");
                }

                if (board.Words.All(w => w.Content != addedWord))
                {
                    board.Words.Add(word.Word);
                    board.Content = newContent;
                    if (word.WordCount > 1)
                    {
                        word.WordCount--;
                    }
                    else
                    {
                        this.Data.WordsUsers.Delete(word);
                    }

                    pointsOfWord = this.GetPoints(addedWord);
                    var boardsUsers = this.Data.BoardsUsers
                        .First(bu => (bu.UserId == userId && bu.Board.Name.Text == boardName));

                    boardsUsers.UserPoints += pointsOfWord;
                    var user = this.Data.Users.First(u => u.Id == userId);

                    this.Data.SaveChanges();

                    userPoits = boardsUsers.UserPoints;
                }
                else
                {
                    throw new ApplicationException(string.Format("The word \"{0}\" is already  embedded",
                        word.Word.Content));
                }
            }
            catch (ApplicationException ex)
            {
                return new
                {
                    isClosed = isBoardClosed,
                    message = ex.Message,
                    points = userPoits
                };
            }
            finally
            {
                var json = JsonConvert.SerializeObject(new
               {
                   content = board.Content,
                   words = board.Words.Select(w => w.Content).ToList()
               });

                this.Clients.Group(boardName).loadBoard(json);
            }

            return new
            {
                message = string.Format("Points of word: {0}", pointsOfWord),
                points = userPoits
            };
        }


        private int GetStartPosition(int boardSize, int wordLength, int catchedLetterId, bool isVertical, int dropCellId)
        {
            int start = 0;
            int end = 0;

            if (isVertical)
            {
                start = dropCellId - catchedLetterId * boardSize;
                end = start + (wordLength - 1) * boardSize;
                if (start < 0 || end >= boardSize * boardSize)
                {
                    return -1;
                }
            }
            else
            {
                start = dropCellId - catchedLetterId;
                end = start + wordLength - 1;

                if ((start / boardSize) != (end / boardSize))
                {
                    return -1;
                }
            }

            return start;
        }

        private void AddWordInBoardContent(Board board, string addedWord, int catchedLetterId, bool isVertical, int dropCellId, int startPosition)
        {
            var s = board.Size;
            var cells = board.Content.ToCharArray();
            this.CrossLetters = new List<char>();

            if (!isVertical)
            {
                s = 1;
            }

            for (var i = 0; i < addedWord.Count(); i++)
            {
                var currentCell = cells[startPosition + i * s];
                if (currentCell != addedWord[i] && currentCell != ' ')
                {
                    this.BoardContent = null;
                    return;
                }

                if (currentCell == addedWord[i])
                {
                    this.CrossLetters.Add(currentCell);
                }
            }

            for (var i = 0; i < addedWord.Count(); i++)
            {
                cells[startPosition + i * s] = addedWord[i];
            }

            this.BoardContent = string.Join("", cells);
        }

        private int GetPoints(string addedWord)
        {
            const double bonusPercentagesForCrossing = 10;

            var points = 0;
            var wordPoints = this.WordAssessor.GetPointsByWord(addedWord);
            var numberOfCrossings = this.CrossLetters.Count;
            var additionalPoints = this.CrossLetters
                .Select(l => this.WordAssessor.GetPointsByLetter(l))
                .Sum();

            points = (int) Math.Round((wordPoints * (1 + bonusPercentagesForCrossing / 100 * numberOfCrossings)), 0) + additionalPoints;

            return points;
        }
    }
}