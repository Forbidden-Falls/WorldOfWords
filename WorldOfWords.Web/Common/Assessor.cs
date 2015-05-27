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

        public static int GetPercentOfFilling(string content)
        {
            var lettersCount = content.Count(letter => letter != ' ');
            var percent = 100 * lettersCount / content.Length;
            return percent;
        }

        public static int GetBonusCoefficientByBoard(string boardContent)
        {
            var fillingPercents = GetPercentOfFilling(boardContent);
            var bonusCoeficient = 1;

            if (Config.FirstBonusLevel < fillingPercents && fillingPercents <= Config.SecondBonusLevel)
            {
                return Config.FirstBonusLevelCoefficient;
            }
            else if (Config.SecondBonusLevel < fillingPercents && fillingPercents <= Config.ThirdBonusLevel)
            {
                return Config.SecondBonusLevelCoefficient;
            }
            else if (fillingPercents > Config.ThirdBonusLevel)
            {
                return Config.ThirdBonusLevelCoefficient;
            }

            return bonusCoeficient;
        }
    }
}