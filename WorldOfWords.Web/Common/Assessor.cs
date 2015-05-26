namespace WorldOfWords.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Data.Contracts;
    using Models;

    public class Assessor
    {
        private List<LettersPoints> lettersPoints;

        public Assessor(string language, IWorldOfWordsData data)
        {
            var languageId = data.Languages
                .First(l => l.LanguageCode == language).Id;

            this.lettersPoints = data.LettersPoints.Where(l => l.LanguageId == languageId).ToList();
        }

        public Assessor(string language)
            : this(language, new WorldOfWordsData())
        {
          
        }

        public int GetPointsByLetter(char letter)
        {
            var letterEntity = this.lettersPoints.FirstOrDefault(l => l.Letter[0] == letter);
            if (letterEntity == null)
            {
                throw new InvalidOperationException("Invalid letter");
            }

            return letterEntity.Points;
        }

        public int GetPointsByWord(string word)
        {
            var wordPoints = word
                .ToCharArray()
                .ToList()
                .Select(this.GetPointsByLetter)
                .Sum();

            return wordPoints;
        }
    }
}