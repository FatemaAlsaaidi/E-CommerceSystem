using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.ReviewDTO;

namespace E_CommerceSystem.Mapping
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<ReviewCreateDto, Review>();
            CreateMap<ReviewUpdateDto, Review>();
            CreateMap<Review, ReviewReadDto>();

           
        }
    }
}