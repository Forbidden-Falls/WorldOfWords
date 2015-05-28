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
            var random = new Random();
            while (balanceInStore < Config.MaxBalanceInStore)
            {
                var randomIndex = random.Next(0, this.Data.Words.Count());
                var ranomLength = GetTypeOfWords();

                var word = this.Data.Words
                   .Where(w => w.Content.Length == ranomLength)
                    .OrderBy(w => Guid.NewGuid())
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
                }
                else
                {
                    storeWord.Quantity += Config.InitialQuantityForWordInStore;
                }

                balanceInStore += this.WordAssessor.GetPointsByWord(word.Content) * Config.InitialQuantityForWordInStore;
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

        private int GetTypeOfWords()
        {
            var totalCount = this.Data.StoreWords.Count();
            totalCount = totalCount < Config.WordsPerPage
                ? Config.WordsPerPage
                : totalCount;

            var storeWords = this.Data.StoreWords
                .GroupBy(sw => sw.Word.Content.Length)
                .Select(g => new
                {
                    Length = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var numbers = storeWords.ToDictionary(word => word.Length, word => word.Count);

            for (int i = 2; i <= 10; i++)
            {
                if (!numbers.ContainsKey(i))
                {
                    numbers.Add(i, 0);
                }
            }

            var typeOfWords = numbers.OrderBy(n => n.Value).First().Key;
            numbers[typeOfWords]++;
            return typeOfWords;
        }
    }
}