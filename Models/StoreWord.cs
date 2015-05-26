namespace Models
{
    using System.ComponentModel.DataAnnotations;
    using System;

    public class StoreWord
    {
        public int Id { get; set; }

        [Required]
        public int WordId { get; set; }

        public virtual Word Word { get; set; }

        public int Quantity { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

    }
}
