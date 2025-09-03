using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ReviewDTO
    {
        [Range(0, 5, ErrorMessage = "The value must be between 0 and 5.")]
        public int Rating { get; set; }

        public string Comment { get; set; } = null;

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Records for AutoMapper
        public record ReviewCreateDto(int Rating, string? Comment, int ProductId, int UserId);
        public record ReviewUpdateDto(int Rating, string? Comment);
        public record ReviewReadDto(int ReviewId, int Rating, string? Comment, int ProductId, int UserId);

    }
}
