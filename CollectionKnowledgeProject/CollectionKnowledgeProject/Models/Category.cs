using System.ComponentModel.DataAnnotations;

namespace CollectionKnowledgeProject.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Question>? Questions { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string? UserId { get; set; }

    }
}
