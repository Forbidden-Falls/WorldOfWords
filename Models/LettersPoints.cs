namespace Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LettersPoints
    {
        public int Id { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        [Column(TypeName = "nchar")]
        [MaxLength(1)]
        public string Letter { get; set; }

        [Required]
        public int Points { get; set; }
    }
}
