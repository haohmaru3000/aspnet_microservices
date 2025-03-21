using MediatR;
using Ordering.Application.Common.Exceptions;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using ILogger = Serilog.ILogger;

namespace Ordering.Application.Features.V1.Orders;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private const string MethodName = "DeleteOrderCommandHandler";

    public async Task Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var orderEntity = await _orderRepository.GetByIdAsync(command.Id);
        if (orderEntity == null) throw new NotFoundException(nameof(Order), command.Id);

        _orderRepository.DeleteOrder(orderEntity);
        orderEntity.DeletedOrder(); // Raise DeletedOrder event
        await _orderRepository.SaveChangesAsync();

        _logger.Information($"Order {orderEntity.Id} was successfully deleted.");
    }
}