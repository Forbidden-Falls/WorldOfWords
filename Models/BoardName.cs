namespace Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class BoardName
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public int LanguageId { get; set; }

        public virtual Language Language { get; set; }

        public int? BoardId { get; set; }

        public virtual Board Board { get; set; }
    }
}
