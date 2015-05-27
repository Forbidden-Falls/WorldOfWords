using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data;
using Data.Contracts;
using Models;

namespace WorldOfWords.Web.Common
{
    public class StoreManager
    {
        private IWorldOfWordsData Data { get; set; }
        private Assessor WordAssessor { get; set; }

        public StoreManager(IWorldOfWordsData data, Assessor assessor)
        {
            this.Data = data;
            this.WordAssessor = assessor;
        }


        private void FillStore(int balanceInStore)
        {
            new LogEvent("In fill store - " + balanceInStore).Raise();
            var random = new Random();
            while (balanceInStore < Config.MaxBalanceInStore)
            {
                new LogEvent("in while - " + balanceInStore + "; max is: " + Config.MaxBalanceInStore + "; check " +
                             (balanceInStore < Config.MaxBalanceInStore));
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
                        DateAdded = DateTime.UtcNow,
                        WordId = word.Id,
                        Quantity = Config.InitialQuantityForWordInStore
                    };

                    this.Data.StoreWords.Add(newStoreWord);
                    balanceInStore += this.WordAssessor.GetPointsByWord(word.Content) * newStoreWord.Quantity;
                }
                else
                {
                    storeWord.Quantity += Config.InitialQuantityForWordInStore;
                }
                this.Data.SaveChanges();
            }
        }

        private int TotalBalanceInStore()
        {
            var words = this.Data.StoreWords.Select(w => new { w.Word.Content, w.Quantity }).ToList();
            var totalBalanceInStore = 0;
            words.ForEach(w => totalBalanceInStore += this.WordAssessor.GetPointsByWord(w.Content) * w.Quantity);

            return totalBalanceInStore;
        }

        public void FillStoreIfNeeded()
        {
            var balanceInStore = TotalBalanceInStore();
            if (balanceInStore < Config.MinBalanceInStore)
            {
                FillStore(balanceInStore);
            }
        }
    }
}