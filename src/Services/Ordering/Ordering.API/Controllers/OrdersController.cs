using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoMapper;
using Contracts.Messages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Ordering.Domain.Entities;
using Shared.SeedWork;

namespace Ordering.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMessageProducer _messageProducer;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    // private readonly ISmtpEmailService _emailService;

    public OrdersController(IMediator mediator, IMessageProducer messageProducer,
        IOrderRepository orderRepository, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _messageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        // _emailService = emailService;
    }

    private static class RouteNames
    {
        public const string GetOrders = nameof(GetOrders);
        public const string CreateOrder = nameof(CreateOrder);
        public const string UpdateOrder = nameof(UpdateOrder);
        public const string DeleteOrder = nameof(DeleteOrder);
    }

    [HttpGet("{username}", Name = RouteNames.GetOrders)]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUsername([Required] string username)
    {
        var query = new GetOrdersQuery(username);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // [HttpPost(Name = RouteNames.CreateOrder)]
    // [ProducesResponseType(typeof(ApiResult<long>), (int)HttpStatusCode.OK)]
    // public async Task<ActionResult<ApiResult<long>>> CreateOrder([FromBody] CreateOrderCommand command)
    // {
    //     var result = await _mediator.Send(command);
    //     return Ok(result);
    // }

    [HttpPut("{id:long}", Name = RouteNames.UpdateOrder)]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<OrderDto>>> UpdateOrder([Required] long id,
        [FromBody] UpdateOrderCommand command)
    {
        command.SetId(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:long}", Name = RouteNames.DeleteOrder)]
    [ProducesResponseType(typeof(NoContentResult), (int)HttpStatusCode.NoContent)]
    public async Task<ActionResult> DeleteOrder([Required] long id)
    {
        var command = new DeleteOrderCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    /** RabbitMQ APIs **/
    [HttpPost]
    public async Task<IActionResult> CreateOrder2(OrderDto orderDto)
    {
        var order = _mapper.Map<Order>(orderDto);
        var addedOrder = await _orderRepository.CreateOrder2(order);

        await _orderRepository.SaveChangesAsync();
        var result = _mapper.Map<OrderDto>(addedOrder);

        await _messageProducer.SendMessage(result); // Publish addedOrder message to RabbitMQ, then consume it

        return Ok(result);
    }

    // [HttpGet("test-email")]
    // public async Task<IActionResult> TestEmail()
    // {
    //     var message = new MailRequest
    //     {
    //         Body = "<h1>Hello</h1>",
    //         Subject = "Testing",
    //         ToAddress = "tuanit168@gmail.com"
    //     };
    //     await _emailService.SendEmailAsync(message);
    //
    //     return Ok();
    // }
}