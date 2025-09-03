using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.ReviewDTO;
public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<ReviewCreateDto, Review>();
        CreateMap<ReviewUpdateDto, Review>();
        CreateMap<Review, ReviewReadDto>();
    }
}