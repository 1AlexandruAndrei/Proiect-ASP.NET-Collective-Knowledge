using System.ComponentModel.DataAnnotations;

namespace CollectionKnowledgeProject.Models
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        [StringLength(4000, ErrorMessage = "Continutul nu poate avea mai mult de 2500 de caractere")]
        [MinLength(5, ErrorMessage = "Continutul trebuie sa aiba mai mult de 5 caractere")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Votes { get; set; }
        public ICollection<Comment>? Comments { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string? UserId { get; set; }
    }
}
