namespace Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class BoardsUsers
    {
        private const int UserIdMaxLength = 128;

        public int Id { get; set; }
        
        public int UserPoints { get; set; }

        [Required,
        MaxLength(UserIdMaxLength)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public int BoardId { get; set; }

        [ForeignKey("BoardId")]
        public virtual Board Board { get; set; }
    }
}
