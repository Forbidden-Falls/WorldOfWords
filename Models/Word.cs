namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Word
    {
        private ICollection<Board> board;
        private ICollection<WordsUsers> users;

        public Word()
        {
            this.board = new HashSet<Board>();
            this.users = new HashSet<WordsUsers>();
        }

        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        public int LanguageId { get; set; }

        public virtual Language Language { get; set; }

        public virtual ICollection<Board> Boards
        {
            get { return this.board; }
        }

        public virtual ICollection<WordsUsers> Users
        {
            get { return this.users; }
        }
    }
}
