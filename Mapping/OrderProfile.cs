using AutoMapper;
using E_CommerceSystem.Models;
using static E_CommerceSystem.Models.OrderItemDTO;
using static E_CommerceSystem.Models.OrdersOutputOTD;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Map order lines -> OrdersOutputOTD using related Order/Product
        CreateMap<OrderProducts, OrdersOutputOTD>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.product.ProductName))
            .ForMember(d => d.OrderDate, o => o.MapFrom(s => s.Order.OrderDate))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.Quantity * s.product.Price));

        // Map OrderProducts to OrderItemDTO
        CreateMap<OrderProducts, OrderItemDTO>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.product.ProductName));

    }
}