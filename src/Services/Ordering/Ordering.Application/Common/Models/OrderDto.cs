using AutoMapper;
using Ordering.Application.Common.Mappings;
using Ordering.Application.Features.V1.Orders;
using Ordering.Domain.Entities;
using Shared.Enums.Order;

namespace Ordering.Application.Common.Models;

public class OrderDto : IMapFrom<Order>, IMapFrom<UpdateOrderCommand>
{
    public long Id { get; set; }
    public string? DocumentNo { get; set; }
    public string? Username { get; set; }
    public decimal TotalPrice { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }

    // Address
    public string? ShippingAddress { get; set; }
    public string? InvoiceAddress { get; set; }

    public EOrderStatus? Status { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Order, OrderDto>().ReverseMap();
    }
}