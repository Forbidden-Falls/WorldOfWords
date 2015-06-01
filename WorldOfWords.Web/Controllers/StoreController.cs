using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.UI;
using Models;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;
using WorldOfWords.Web.BindingModels;

namespace WorldOfWords.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using PagedList;
    using WorldOfWords.Web.Common;
    using WorldOfWords.Web.ViewsModels;

    [Authorize]
    public class StoreController : BaseController
    {
        public ActionResult Index(string sortOrder, string searchString, string currentFilter, int? page)
        {
            return View();
        }

        public ActionResult Store(string sortOrder, string searchString, string currentFilter, int? page)
        {
            ViewBag.Assessor = this.WordAssessor;

            var currentUser = User.Identity.GetUserId();
            var words = this.Data.StoreWords
                .Where(w => w.Word.Language.LanguageCode == Config.Language)
                .Select(sw => new WordWithCount
                {
                    Id = sw.Id,
                    Content = sw.Word.Content,
                    Quantity = sw.Quantity,
                    QuantityUser = sw.Word.Users.FirstOrDefault(u => u.UserId == currentUser && u.WordId == sw.Id).WordCount,
                    LanguageId = sw.Word.LanguageId,
                    DateAdded = sw.DateAdded
                })
                .AsQueryable();

            #region Search
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                words = words.Where(w => w.Content.Contains(searchString));
            }
            #endregion

            #region Sorting
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.LengthSortParam = sortOrder == "Length" ? "length_desc" : "Length";

            switch (sortOrder)
            {
                case "name_desc":
                    words = words.OrderByDescending(w => w.Content);
                    break;
                case "Date":
                    words = words.OrderBy(w => w.DateAdded);
                    break;
                case "date_desc":
                    words = words.OrderByDescending(w => w.DateAdded);
                    break;
                case "Length":
                    words = words.OrderBy(w => w.Content.Length);
                    break;
                case "length_desc":
                    words = words.OrderByDescending(w => w.Content.Length);
                    break;
                default:  // Name ascending 
                    words = words.OrderBy(s => s.Content);
                    break;
            }
            #endregion

            int pageNumber = (page ?? 1);
            return PartialView(words.ToPagedList(pageNumber, Config.WordsPerPage));
        }

        [HttpPost]
        public ActionResult Cart(List<ShopItem> shopList)
        {
            if (shopList == null) return null;

            if (shopList.Any(w => w.Quantity < 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid quantity");
            }

            if (shopList.Any(sl => this.Data.StoreWords.FirstOrDefault(w => w.Id == sl.WordId) == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "This word isn't available at the moment");
            }

            var cartItems = shopList.Select(sl => new CartItem()
            {
                WordId = sl.WordId,
                Word = this.Data.StoreWords.FirstOrDefault(w => w.Id == sl.WordId).Word.Content,
                Quantity = sl.Quantity
            }).ToList();

            foreach (var item in cartItems)
            {
                item.Price = this.WordAssessor.GetPointsByWord(item.Word);
            }

            this.ViewBag.TotalPrice = cartItems.Sum(i => i.Price * i.Quantity);
            return PartialView(cartItems);
        }

        public ActionResult UpdateCart(List<ShopItem> shopList)
        {
            this.Session["shopList"] = JsonConvert.SerializeObject(shopList);

            return Json(new { shopList }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Buy(List<ShopItem> shopList)
        {
            var errors = new List<string>();
            var currentUserId = this.User.Identity.GetUserId();
            var userDb = this.Data.Users.FirstOrDefault(u => u.Id == currentUserId);

            var shopListWordIds = shopList.Select(sl => sl.WordId);
            var storeWords = this.Data.StoreWords
                .Where(sw => shopListWordIds.Contains(sw.Id))
                .Include(sw => sw.Word)
                .ToList();

            var totalPriceForWords = TotalPriceForWords(shopList, storeWords);
            if (totalPriceForWords > userDb.Balance)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Not enough balance");
            }

            FilterMissingWordsInShopList(ref shopList, shopListWordIds, storeWords, errors);
            if (shopList.Count == 0)
            {
                this.Session["shopList"] = null;
                return Json(new { errors, balance = userDb.Balance }, JsonRequestBehavior.AllowGet);
            }

            BuyWords(shopList, storeWords, errors, userDb);

            this.Data.SaveChanges();

            Task.Factory.StartNew(() => this.StoreManager.FillStoreIfNeeded());

            this.Session["shopList"] = null;
            return Json(new { errors, balance = userDb.Balance }, JsonRequestBehavior.AllowGet);
        }

        private void FilterMissingWordsInShopList(ref List<ShopItem> shopList,
            IEnumerable<int> shopListWordIds,
            List<StoreWord> storeWords,
            List<string> errors)
        {
            var missingWordsInStore = shopListWordIds.Except(storeWords.Select(sw => sw.Id));
            if (missingWordsInStore.Any())
            {
                var missingWords = shopList.Where(sl => missingWordsInStore.Contains(sl.WordId));

                shopList = shopList.Except(missingWords).ToList();
                errors.Add("Some words were not available: " + string.Join(", ", missingWords.Select(sl => sl.Word)));
            }
        }

        private void BuyWords(List<ShopItem> shopList, List<StoreWord> storeWords, List<string> errors, User user)
        {
            foreach (var storeWord in storeWords)
            {
                var shopItem = shopList.First(si => si.WordId == storeWord.Id);
                if (storeWord.Quantity < shopItem.Quantity)
                {
                    shopItem.Quantity = storeWord.Quantity;
                    errors.Add(String.Format("There isn't enough quantity for word {0}. {1} bought instead",
                        storeWord.Word.Content, shopItem.Quantity));
                }

                var userWord = user.WordsUsers.FirstOrDefault(w => w.WordId == storeWord.WordId);
                if (userWord != null)
                {
                    userWord.WordCount += shopItem.Quantity;
                }
                else
                {
                    user.WordsUsers.Add(new WordsUsers()
                    {
                        WordId = storeWord.WordId,
                        WordCount = shopItem.Quantity
                    });
                }

                storeWord.Quantity -= shopItem.Quantity;

                if (storeWord.Quantity == 0)
                {
                    this.Data.StoreWords.Delete(storeWord);
                }

                var spentMoney = this.WordAssessor.GetPointsByWord(storeWord.Word.Content) * shopItem.Quantity;
                user.Balance -= spentMoney;
                user.Statistics.SpentMoney += spentMoney;
            }
        }

        private int TotalPriceForWords(List<ShopItem> shopList, List<StoreWord> storeWords)
        {
            int totalPriceForWords = 0;

            foreach (var storeWord in storeWords)
            {
                var price = this.WordAssessor.GetPointsByWord(storeWord.Word.Content);
                var quantity = shopList.First(sl => sl.WordId == storeWord.Id).Quantity;

                totalPriceForWords += price * quantity;
            }

            return totalPriceForWords;
        }
    }
}