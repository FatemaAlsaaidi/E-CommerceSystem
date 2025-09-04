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

        // Records for AutoMapper
        public record CategoryCreateDto(string Name, string? Description);
        public record CategoryUpdateDto(string Name, string? Description);
        public record CategoryReadDto(int CategoryId, string Name, string? Description);


    }


}