using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldOfWords.Web.BindingModels
{
    public class ShopItem
    {
        public int WordId { get; set; }
        public string Word { get; set; }
        public int Quantity { get; set; }
    }
}