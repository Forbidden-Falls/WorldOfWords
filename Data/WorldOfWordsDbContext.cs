namespace Data
{
    using System.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;

    public class WorldOfWordsDbContext : IdentityDbContext<User>
    {
        public WorldOfWordsDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Board> Boards { get; set; }

        public DbSet<Word> Words { get; set; }

        public DbSet<StoreWord> StoreWords { get; set; }

        public DbSet<WordsUsers> WordsUsers { get; set; }

        public DbSet<BoardsUsers> UsersBoards { get; set; }

        public DbSet<LettersPoints> LettersPoints { get; set; }

        public DbSet<Language> Languages { get; set; }

        public DbSet<BoardName> BoardNames { get; set; }

        public DbSet<Statistics> Statistics { get; set; }

        public static WorldOfWordsDbContext Create()
        {
            return new WorldOfWordsDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<BoardName>()
            //    .HasKey(e => e.Id);

            modelBuilder.Entity<Board>()
                .HasRequired(b => b.Name) 
                .WithOptional(n => n.Board); 

            modelBuilder.Entity<Statistics>()
                .HasRequired(s => s.User); 

            base.OnModelCreating(modelBuilder);
        }
    }
}
