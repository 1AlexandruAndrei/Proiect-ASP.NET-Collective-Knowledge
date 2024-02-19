using Microsoft.AspNetCore.Identity;

namespace CollectionKnowledgeProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePhotoPath { get; set; }
    }
}
