using AutoMapper;
using Contracts.Services;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Shared.SeedWork;
using Shared.Services.Email;
using ILogger = Serilog.ILogger;

namespace Ordering.Application.Features.V1.Orders;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResult<long>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ISmtpEmailService _emailService;
    private readonly ILogger _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        ISmtpEmailService emailService,
        ILogger logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private const string MethodName = "CreateOrderCommandHandler";

    public async Task<ApiResult<long>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        _logger.Information($"BEGIN: {MethodName} - Username: {command.Username}");

        var orderEntity = _mapper.Map<Order>(command);
        // var addedOrder = await _orderRepository.CreateOrderAsync(orderEntity);

        _orderRepository.CreateOrder(orderEntity);
        orderEntity.AddedOrder();
        await _orderRepository.SaveChangesAsync(); // want to publish entity that's developed with BaseEvent atm

        _logger.Information($"Order: {orderEntity.Id} - Document No: {orderEntity.DocumentNo} was successfully created.");

        // SendEmailAsync(addedOrder, cancellationToken);

        _logger.Information($"END: {MethodName} - Username: {command.Username}");
        return new ApiSuccessResult<long>(orderEntity.Id);
    }

    private async Task SendEmailAsync(Order order, CancellationToken cancellationToken)
    {
        var emailRequest = new MailRequest
        {
            ToAddress = order.EmailAddress,
            Body = "Order was created.",
            Subject = "Order was created"
        };

        try
        {
            await _emailService.SendEmailAsync(emailRequest, cancellationToken);
            _logger.Information($"Sent Created Order to email {order.EmailAddress}");
        }
        catch (Exception ex)
        {
            _logger.Error($"Order {order.Id} failed due to an error with the email service: {ex.Message}");
        }
    }
}