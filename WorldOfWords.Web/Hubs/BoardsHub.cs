namespace WorldOfWords.Web.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
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
                    throw new ApplicationException("Полето е затворено");
                }

                var boardsUsers = this.Data.BoardsUsers
                    .FirstOrDefault(bu => (bu.UserId == userId && bu.Board.Name.Text == boardName));
                if (boardsUsers == null)
                {
                    board.BoardsUsers.Add(new BoardsUsers
                    {
                        BoardId = board.Id,
                        UserId = userId,
                    });

                    this.Data.SaveChanges();

                    boardsUsers = this.Data.BoardsUsers
                    .FirstOrDefault(bu => (bu.UserId == userId && bu.Board.Name.Text == boardName));
               }

                userPoits = boardsUsers.UserPoints;

                var word = this.Data.WordsUsers
                    .Where(wu => wu.UserId == userId)
                    .FirstOrDefault(w => w.Word.Content == addedWord);
                if (word == null)
                {
                    throw new ApplicationException("Нямаш таква дума.");
                }

                var startPosition = this.GetStartPosition(board.Size, addedWord.Count(), catchedLetterId, isVertical,
                    dropCellId);

                if (startPosition < 0)
                {
                    throw new ApplicationException("Думата е извън полето.");
                }

                this.AddWordInBoardContent(board, addedWord, catchedLetterId, isVertical, dropCellId, startPosition);
                var newContent = this.BoardContent;

                if (newContent == null)
                {
                    throw new ApplicationException("Непозволено пресичане на думи.");
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

                    pointsOfWord = this.GetPoints(addedWord, board.Content);

                    boardsUsers.UserPoints += pointsOfWord;
                    var user = this.Data.Users.First(u => u.Id == userId);

                    this.Data.SaveChanges();

                    userPoits = boardsUsers.UserPoints;
                }
                else
                {
                    throw new ApplicationException(string.Format("Думата \"{0}\" е има в полето.",
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

                this.Clients.Group(boardName).loadBoard(json, Assessor.GetPercentOfFilling(board.Content));
            }

            return new
            {
                message = string.Format("Изкарани точки от думата: {0}", pointsOfWord),
                points = userPoits,
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

        private int GetPoints(string addedWord, string boardContent)
        {
            const double bonusPercentagesForCrossing = 10;

            var points = 0;
            var wordPoints = this.WordAssessor.GetPointsByWord(addedWord);
            var numberOfCrossings = this.CrossLetters.Count;
            var additionalPoints = this.CrossLetters
                .Select(l => this.WordAssessor.GetPointsByLetter(l))
                .Sum();
            var bonusCoefficient = Assessor.GetBonusCoefficientByBoard(boardContent);

            points = wordPoints 
                + ((int)(Math.Round((wordPoints * (bonusPercentagesForCrossing / 100 * numberOfCrossings)), 0) + additionalPoints)) * bonusCoefficient;
            return points;
        }
    }
}