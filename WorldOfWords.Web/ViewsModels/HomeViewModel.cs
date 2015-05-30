namespace WorldOfWords.Web.ViewsModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    public class HomeViewModel
    {
        public string UserName { get; set; }

        public long EarnedPoints { get; set; }

        public int Balance { get; set; }

        public DateTime RegisteredOn { get; set; }

        public virtual List<IGrouping<char, WordsUsers>> WordsUsers { get; set; }

        public virtual ICollection<BoardsUsers> BoardsUsers { get; set; }

        public List<Board> NonUserBoards { get; set; }

        public List<Board> AllBoards { get; set; }
    }

    public class RaitingViewModel
    {
        public int UserPlaceBalance { get; set; }

        public List<User> UsersStatsBalance { get; set; }

        public int UserPlacePoints { get; set; }

        public List<User> UsersStatsPoints { get; set; }

        public int UserPlaceSpendMoney { get; set; }

        public List<User> UsersStatsSpendMoney { get; set; }

        public int UserPlaceMostPointOfWord { get; set; }

        public List<User> UsersStatsMostPointOfWord { get; set; }

        public User LoggedUser { get; set; }
    }
}