namespace Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WordsUsers
    {
        private const int UserIdMaxLength = 128;
        private const int DefaultWordCountValue = 1;

        public int Id { get; set; }

        [DefaultValue(DefaultWordCountValue)]
        public int WordCount { get; set; }

        [Required,
        MaxLength(UserIdMaxLength)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int WordId { get; set; }

        [ForeignKey("WordId")]
        public virtual Word Word { get; set; }
    }
}
