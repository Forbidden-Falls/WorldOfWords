namespace WorldOfWords.Web.ViewsModels
{
    public class UserStatisticsViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public int Balance { get; set; }

        public int? MostPointsOfWord { get; set; }

        public long SpentMoney { get; set; }

        public long EarnedPoints { get; set; }

        public string Email { get; set; }
    }
}