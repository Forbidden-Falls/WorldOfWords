namespace WorldOfWords.Web.Common
{
    public static class Config
    {
        public const string Language = "bg";
        public const int MaxNumberOfBoards = 10;

        // board size
        public const int MinSize = 5;
        public const int MaxSize = 10;

        // board life
        public const int MinDurationInMinutes = 5;
        // 1 day = 60 min * 24 h
        public const int MaxDurationInMinutes = 15;
    }
}