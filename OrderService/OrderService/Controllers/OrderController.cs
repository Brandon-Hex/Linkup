using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using OrderService.DTO;
using OrderService.Entities;
using OrderService.Persistence;
using OrderService.Utils;
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Controllers;

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMapper _mapper;
    private readonly RabbitMqService _rabbitMqService;
    private readonly IServiceProvider _serviceProvider;

    public OrderController(ILogger<OrderController> logger, AppDbContext dbContext, IMapper mapper, RabbitMqService rabbitMqService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _appDbContext = dbContext;
        _mapper = mapper;
        _rabbitMqService = rabbitMqService;
        _serviceProvider = serviceProvider;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDto)
    {
        if (createOrderDto == null)
        {
            return BadRequest("No Order specified.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var mappedEntity = _mapper.Map<OrderEntity>(createOrderDto);

            _appDbContext.Orders.Add(mappedEntity);
            await _appDbContext.SaveChangesAsync();

            var userDetails = await RabbitMQUtils.requestUserDetailsAsync(_serviceProvider, createOrderDto.UserId);
            _logger.LogInformation(userDetails.Email);

            var orderNotification = new SendOrderNotificationDTO()
            {
                OrderDate = DateTime.Now,
                UserId = createOrderDto.UserId,
                Product = createOrderDto.Product,
                TotalAmount = createOrderDto.TotalAmount,
                UserEmail = userDetails.Email,
                UserName = userDetails.Name
            };

            RabbitMQUtils.sendOrderNotification(_serviceProvider, orderNotification);

            return CreatedAtRoute("GetOrderById", new { id = mappedEntity.Id }, mappedEntity);
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23503")
        {
            //Handle foreign key constraint violation
            _logger.LogError("Foreign key constraint violation: {Message}", postgresEx.Message);
            return BadRequest("The specified user ID does not exist. Please provide a valid user ID.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<ActionResult<OrderEntity>> GetOrderById(int id)
    {
        var order = await _appDbContext.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet(Name = "GetAllOrders")]
    public async Task<ActionResult> GetAllOrders()
    {
        try
        {
            return Ok(await _appDbContext.Orders.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO updateOrderDto)
    {
        if (updateOrderDto == null)
        {
            return BadRequest("No Order data specified.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingOrder = await _appDbContext.Orders.FindAsync(id);

        if (existingOrder == null)
        {
            return NotFound();
        }

        try
        {
            _mapper.Map(updateOrderDto, existingOrder);

            _appDbContext.Orders.Update(existingOrder);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _appDbContext.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        try
        {
            _appDbContext.Orders.Remove(order);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }
}
