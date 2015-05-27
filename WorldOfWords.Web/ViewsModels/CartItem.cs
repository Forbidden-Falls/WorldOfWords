namespace WorldOfWords.Web.ViewsModels
{
    public class CartItem
    {
        public int WordId { get; set; }
        public string Word { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}