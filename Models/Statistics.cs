namespace Models
{
    public class Statistics
    {
        public string Id { get; set; }

        public long?  SpentMoney { get; set; }

        public int? MostPointsOfWord { get; set; }

        public virtual User User { get; set; }
    }
}
