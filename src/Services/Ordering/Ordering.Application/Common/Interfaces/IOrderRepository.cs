using Contracts.Domains.Interfaces;
using Ordering.Domain.Entities;

namespace Ordering.Application.Common.Interfaces;

public interface IOrderRepository : IRepositoryBase<Order, long>
{
    Task<IEnumerable<Order>> GetOrdersByUsernameAsync(string username);
    void CreateOrder(Order order);
    Task<Order> UpdateOrderAsync(Order order);
    void DeleteOrder(Order order);
}