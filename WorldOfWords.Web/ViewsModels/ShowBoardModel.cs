namespace WorldOfWords.Web.ViewsModels
{
    using System.Collections.Generic;
    using Models;

    public class ShowBoardModel
    {
        public Board Board { get; set; }

        public int UserPointsFromBoard { get; set; }

        public long EarnedUserPoints { get; set; }

        public IList<string> UserWords { get; set; }

        public IList<int> WordsPoints { get; set; }
    }
}
