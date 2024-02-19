using System.ComponentModel.DataAnnotations;

namespace CollectionKnowledgeProject.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        [StringLength(4000, ErrorMessage = "Continutul nu poate avea mai mult de 4000 de caractere")]
        [MinLength(5, ErrorMessage = "Continutul trebuie sa aiba mai mult de 5 caractere")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Votes { get; set; }
        public int? QuestionId { get; set; }
        public Question? Question { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string? UserId { get; set; }

    }
}
