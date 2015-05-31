namespace Models
{
    using System.ComponentModel;

    public class Statistics
    {
        public Statistics()
        {
            this.MostPointsOfWord = 0;
            this.SpentMoney = 0;
        }

        public string Id { get; set; }

        [DefaultValue(0)]
        public long SpentMoney { get; set; }

        [DefaultValue(0)]
        public int MostPointsOfWord { get; set; }

        public virtual User User { get; set; }
    }
}
