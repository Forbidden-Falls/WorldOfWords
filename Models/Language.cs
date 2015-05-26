namespace Models
{
    using System.Collections.Generic;

    public class Language
    {
        private ICollection<Word> words;
        public Language()
        {
            this.words = new HashSet<Word>();   
        }
        public int Id { get; set; }

        public string LanguageCode { get; set; }

        public virtual ICollection<Word> Words
        {
            get { return this.words; }
        } 
    }
}
