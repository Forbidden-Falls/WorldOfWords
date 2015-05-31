namespace Models
{
    public class Statistics
    {
        public Statistics()
        {
            this.MostPointsOfWord = 0;
            this.SpentMoney = 0;
        }

        public string Id { get; set; }

        public long  SpentMoney { get; set; }

        public int MostPointsOfWord { get ; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
