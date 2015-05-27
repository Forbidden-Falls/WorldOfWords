namespace Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using EntityFramework.Extensions;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;

    public sealed class Configuration : DbMigrationsConfiguration<WorldOfWordsDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(WorldOfWordsDbContext context)
        {
            AddLanguagesToDb(context);
            AddRolesToDb(context);
            AddWordsToDbAndStore(context);
            AddBoardNames(context);
            AddLettersPoints(context);
        }


        private void AddLettersPoints(WorldOfWordsDbContext context)
        {
            if (context.LettersPoints.Any())
            {
                return;
            }

            var language = context.Languages.FirstOrDefault(l => l.LanguageCode == "bg");

            var letters = "абвгдежзийклмнопрстуфхцчшщъьюя";
            var points =
                "1,2,1,3,2,1,4,4,1,5,2,2,2,1,1,1,1,1,1,5,10,5,8,5,8,10,3,10,10,5, 6".Split(',').ToList().Select(int.Parse).ToList();

            for (int i = 0; i < letters.Length; i++)
            {
                var l = new LettersPoints { LanguageId = language.Id, Letter = letters[i].ToString(), Points = points[i] };
                context.LettersPoints.Add(l);
            }

            context.SaveChanges();
        }

        private void AddBoardNames(WorldOfWordsDbContext context)
        {
            if (context.BoardNames.Any())
            {
                return;
            }

            var names = new List<string>
            {
                "София",
                "Варна",
                "Петрич",
                "Русе",
                "Лазур",
                "Синеморец",
                "Лозен",
                "Ахтопол",
                "Приморско",
                "Черноморец",
                "Враня",
                "Средец",
                "Пловдив",
                "Априлово",
                "Маргарита",
                "Синчец",
                "Божур",
                "Рожен",
                "Рила",
                "Мусала",
                "Вихрен",
                "Рай",
                "Пещера",
                "Стара Планина",
                "Люлин",
                "Надежда",
                "Загора",
                "Равно поле",
                "Тръново",
                "Змейово",
            };

            var languageId = context.Languages.First(l => l.LanguageCode == "bg").Id;
            foreach (var name in names)
            {
                context.BoardNames.Add(new BoardName
                {
                    Text = name,
                    LanguageId = languageId
                });
            }

            context.SaveChanges();
        }

        private void AddRolesToDb(WorldOfWordsDbContext context)
        {
            var storeRole = new RoleStore<IdentityRole>(context);
            var managerRole = new RoleManager<IdentityRole>(storeRole);
            var adminRole = new IdentityRole { Name = "admin" };
            var userRole = new IdentityRole { Name = "user" };
            var moderatorRole = new IdentityRole { Name = "moderator" };
            managerRole.Create(adminRole);
            managerRole.Create(userRole);
            managerRole.Create(moderatorRole);

            var storeUser = new UserStore<User>(context);
            var managerUser = new UserManager<User>(storeUser);

            var admin = new User { UserName = "admin@admin.a", Email = "admin@admin.a" };
            var moderator = new User { UserName = "moderator@mod.m", Email = "moderator@mod.m" };

            var resultUser = managerUser.Create(admin, "Aa#123456");
            var resultmod = managerUser.Create(moderator, "Aa#123456");
            context.SaveChanges();

            var adminResult = context.Users.FirstOrDefault(x => x.UserName == "admin@admin.a");
            var moderatorResult = context.Users.FirstOrDefault(x => x.UserName == "moderator@mod.m");
            managerUser.AddToRole(adminResult.Id, "admin");
            managerUser.AddToRole(moderatorResult.Id, "moderator");
            context.SaveChanges();

        }

        private void AddLanguagesToDb(WorldOfWordsDbContext context)
        {
            if (context.Languages.Any()) return;

            var languageCodes = new string[] {"bg"};
            foreach (var languageCode in languageCodes)
            {
                context.Languages.Add(new Language()
                {
                    LanguageCode = languageCode
                });
            }

            context.SaveChanges();
        }

        private void AddWordsToDbAndStore(WorldOfWordsDbContext context)
        {
            if (context.Words.Any()) return;

            var words = new string[]
            {
                "котка",
                "куче",
                "ехо",
                "ябълка",
                "круша",
                "слива",
                "дънки",
                "ластик",
                "жаба",
                "комбинезон",
                "абрихт",
                "комбинирам",
                "ягода",
                "щайга",
                "шайба",
                "ухо",
                "пиле",
                "драка",
                "акула",
            };

            var language = context.Languages.FirstOrDefault(l => l.LanguageCode == "bg");
            var admin = context.Users.FirstOrDefault(x => x.UserName == "admin@admin.a");
            foreach (var word in words)
            {
                var wordEntity = new Word
                {
                    Content = word,
                    DateAdded = DateTime.Now,
                    Language = language
                };
                context.Words.Add(wordEntity);
                context.StoreWords.Add(new StoreWord
                {
                    DateAdded = DateTime.Now,
                    Word = wordEntity,
                    Quantity = 2
                });
                admin.WordsUsers.Add(new WordsUsers
                {
                    Word = wordEntity,
                    WordCount = 2,
                });
            }

            context.SaveChanges();
        }
    }
}
