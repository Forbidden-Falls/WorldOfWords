namespace WorldOfWords.Web.Hubs
{
    using System.Linq;
    using Common;
    using Data;
    using Data.Contracts;
    using Microsoft.AspNet.SignalR;

    public abstract class BaseHub : Hub
    {
        protected BaseHub()
        {
            var context = new WorldOfWordsDbContext();
            this.Data = new WorldOfWordsData(context);
            this.WordAssessor = new Assessor(Config.Language, this.Data);
        }

        protected IWorldOfWordsData Data { get; set; }

        protected Assessor WordAssessor { get; set; }
    }
}