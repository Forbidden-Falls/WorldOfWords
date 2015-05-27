namespace WorldOfWords.Web.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;
    using BindingModels;
    using Common;
    using Data;
    using Data.Contracts;
    using Data.Migrations;
    using Microsoft.Ajax.Utilities;
    using Models;

    public abstract class BaseController : Controller
    {
        protected BaseController()
        {
            var context = new WorldOfWordsDbContext();
            this.Data = new WorldOfWordsData(context);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<WorldOfWordsDbContext, Configuration>());
            
            this.WordAssessor = new Assessor(Config.Language, this.Data);
            var boardManager = new BoardsManager(this.Data);
            boardManager.Execute();

            this.StoreManager = new StoreManager(this.Data, this.WordAssessor);
        }

        protected IWorldOfWordsData Data { get; set; }

        protected Assessor WordAssessor { get; set; }

        protected StoreManager StoreManager { get; set; }
    }
}