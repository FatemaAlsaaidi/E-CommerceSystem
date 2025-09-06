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
            CreateMap<ReviewDTO, Review>()
            .ForMember(d => d.ReviewID, o => o.Ignore())
            .ForMember(d => d.PID, o => o.MapFrom((src, dest, _, ctx) => (int)ctx.Items["pid"]))
            .ForMember(d => d.UID, o => o.MapFrom((src, dest, _, ctx) => (int)ctx.Items["uid"]))
            .ForMember(d => d.ReviewDate, o => o.MapFrom(_ => DateTime.UtcNow));

        }
    }
}