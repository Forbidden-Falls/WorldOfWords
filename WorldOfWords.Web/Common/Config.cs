namespace WorldOfWords.Web.Common
{
    public static class Config
    {
        public const string Language = "bg";

        public const int MaxNumberOfEmptyBoards = 3;
        public const int FillingPercent = 70;

        // board size
        public const int MinSize = 5;
        public const int MaxSize = 10;

        // board life
        public const int MinDurationInMinutes = 10;
        // 2 hours = 60 min * 2 = 120
        public const int MaxDurationInMinutes = 120;

        // Bonus coefficients for filling boards
        public const int FirstBonusLevelCoefficient = 2;
        public const int SecondBonusLevelCoefficient = 3;
        public const int ThirdBonusLevelCoefficient = 4;

        // Filling percents for each bonus level
        public const int FirstBonusLevel = 60;
        public const int SecondBonusLevel = 70;
        public const int ThirdBonusLevel = 85;


        //Store configuration
        public const int WordsPerPage = 6;

        //Store fill configuration
        public const int MinBalanceInStore = 200;
        public const int MaxBalanceInStore = 400;
        public const int InitialQuantityForWordInStore = 2;

    }
}