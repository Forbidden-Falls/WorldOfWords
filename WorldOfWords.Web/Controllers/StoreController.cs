using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using Models;
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
        private const int PageSize = 3;

        public ActionResult Index(string sortOrder, string searchString, string currentFilter, int? page)
        {
            return View();
        }

        public ActionResult Store(string sortOrder, string searchString, string currentFilter, int? page)
        {
            ViewBag.Assessor = this.WordAssessor;

            var currentUser = User.Identity.GetUserId();
            var words = this.Data.StoreWords
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
                default:  // Name ascending 
                    words = words.OrderBy(s => s.Content);
                    break;
            }
            #endregion

            int pageNumber = (page ?? 1);
            return PartialView(words.ToPagedList(pageNumber, PageSize));
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

        public ActionResult Buy(List<ShopItem> shopList)
        {
            var errors = new List<string>();
            var currentUserId = this.User.Identity.GetUserId();
            var userDb = this.Data.Users.FirstOrDefault(u => u.Id == currentUserId);
            var shopListWordIds = shopList.Select(sl => sl.WordId);
            var storeWords = this.Data.StoreWords.Where(sw => shopListWordIds.Contains(sw.Id)).Include(sw => sw.Word).ToList();

            var totalPriceForWords = 0;
            foreach (var storeWord in storeWords)
            {
                var price = this.WordAssessor.GetPointsByWord(storeWord.Word.Content);
                var quantity = shopList.FirstOrDefault(sl => sl.WordId == storeWord.Id).Quantity;

                totalPriceForWords += price*quantity;
            }

            if (totalPriceForWords > userDb.Balance)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Not enough balance");
            }

            var missingWordsInStore = shopListWordIds.Except(storeWords.Select(sw => sw.Id));
            if (missingWordsInStore.Any())
            {
                var missingWords = shopList.Where(sl => missingWordsInStore.Contains(sl.WordId));

                shopList = shopList.Except(missingWords).ToList();
                errors.Add("Some words were not available: " + string.Join(", ", missingWords.Select(sl => sl.Word)));

                if (shopList.Count == 0)
                {
                    return Json(new { errors, balance = userDb.Balance }, JsonRequestBehavior.AllowGet);
                }
            }

            foreach (var storeWord in storeWords)
            {
                var shopItem = shopList.FirstOrDefault(si => si.WordId == storeWord.Id);
                if (storeWord.Quantity < shopItem.Quantity)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There isn't enough quantity for word: " + storeWord.Word.Content);
                }

                var userWord = userDb.WordsUsers.FirstOrDefault(w => w.WordId == storeWord.WordId);
                if (userWord != null)
                {
                    userWord.WordCount += shopItem.Quantity;
                }
                else
                {
                    userDb.WordsUsers.Add(new WordsUsers()
                    {
                        WordId = storeWord.WordId,
                        WordCount = shopItem.Quantity
                    });
                }

                storeWord.Quantity -= shopItem.Quantity;
                userDb.Balance -= this.WordAssessor.GetPointsByWord(storeWord.Word.Content) * shopItem.Quantity;

                if (storeWord.Quantity == 0)
                {
                    this.Data.StoreWords.Delete(storeWord);
                }
            }

            this.Data.SaveChanges();

            Task.Factory.StartNew(() => FillStoreIfNeeded());

            return Json(new { errors, balance = userDb.Balance }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FillStoreIfNeeded()
        {
            var balanceInStore = TotalBalanceInStore();
            if (balanceInStore < Config.MinBalanceInStore)
            {
                var random = new Random();
                while (balanceInStore < Config.MaxBalanceInStore)
                {
                    var randomIndex = random.Next(0, this.Data.Words.Count());

                    var word = this.Data.Words
                        .OrderBy(w => Guid.NewGuid())
                        .Skip(randomIndex)
                        .Take(1)
                        .First();

                    var storeWord = this.Data.StoreWords.FirstOrDefault(sw => sw.WordId == word.Id);
                    if (storeWord == null)
                    {
                        var newStoreWord = new StoreWord()
                        {
                            DateAdded = DateTime.Now,
                            WordId = word.Id,
                            Quantity = Config.InitialQuantityForWordInStore
                        };

                        this.Data.StoreWords.Add(newStoreWord);
                        balanceInStore += this.WordAssessor.GetPointsByWord(word.Content)*newStoreWord.Quantity;
                    }
                    else
                    {
                        storeWord.Quantity += Config.InitialQuantityForWordInStore;
                    }
                    this.Data.SaveChanges();
                }

                return Json(new { success = 1 });
            }
            else
            {
                return Json(new { success = 0, errors = "Store doesn't have to be filled" }, JsonRequestBehavior.AllowGet);
            }
        }

        private int TotalBalanceInStore()
        {
            var words = this.Data.StoreWords.Select(w => new {w.Word.Content, w.Quantity}).ToList();
            var totalBalanceInStore = 0;
            words.ForEach(w => totalBalanceInStore += this.WordAssessor.GetPointsByWord(w.Content)*w.Quantity);

            return totalBalanceInStore;
        }
    }
}