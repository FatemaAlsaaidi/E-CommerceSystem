using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class CategoryDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int CategoryId { get; set; }

    }


}