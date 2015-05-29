using Data.Repositories;
using Models;

namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    using Data.Contracts;


    public class WorldOfWordsData : IWorldOfWordsData
    {
        private readonly DbContext context;

        private readonly IDictionary<Type, object> repositories;

        public WorldOfWordsData(DbContext context)
        {
            this.context = context;
            this.repositories = new Dictionary<Type, object>();
        }

        public WorldOfWordsData()
            : this(new WorldOfWordsDbContext())
        {
        }

        public EFRepository<User> Users
        {
            get { return this.GetGenericRepository<User>(); }
        }

        public EFRepository<Board> Boards
        {
            get { return this.GetGenericRepository<Board>(); }
        }

        public EFRepository<LettersPoints> LettersPoints
        {
            get { return this.GetGenericRepository<LettersPoints>(); }
        }

        public EFRepository<Language> Languages
        {
            get { return this.GetGenericRepository<Language>(); }
        }

        public EFRepository<Word> Words
        {
            get { return this.GetGenericRepository<Word>(); }
        }

        public EFRepository<WordsUsers> WordsUsers
        {
            get { return this.GetGenericRepository<WordsUsers>(); }
        }

        public EFRepository<StoreWord> StoreWords
        {
            get { return this.GetGenericRepository<StoreWord>(); }
        }

        public EFRepository<BoardsUsers> BoardsUsers
        {
            get { return this.GetGenericRepository<BoardsUsers>(); }
        }

        public EFRepository<BoardName> BoardName
        {
            get { return this.GetGenericRepository<BoardName>(); }
        }

        public EFRepository<Statistics> Statistics
        {
            get { return this.GetGenericRepository<Statistics>(); }
        }

        public void SaveChanges()
        {
            this.context.SaveChanges();
        }

        public EFRepository<T> GetGenericRepository<T>() where T : class
        {
            return (EFRepository<T>)this.GetRepository<T>();
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            var typeOfModel = typeof (T);

            if (this.repositories.ContainsKey(typeOfModel))
            {
                return (IRepository<T>) this.repositories[typeOfModel];
            }

            var type = typeof (EFRepository<T>);

            this.repositories.Add(typeOfModel, Activator.CreateInstance(type, this.context));

            return (IRepository<T>) this.repositories[typeOfModel];
        }
    }
}