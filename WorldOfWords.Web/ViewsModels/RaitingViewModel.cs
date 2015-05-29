namespace WorldOfWords.Web.ViewsModels
{
    using System.Collections.Generic;

    using Models;

    public class RaitingViewModel
    {
        public List<User> UsersStatsBalance { get; set; }

        public List<User> UsersStatsPoints { get; set; }

        public User LoggedUser { get; set; }
    }
}