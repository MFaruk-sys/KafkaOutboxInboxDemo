using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using System.Text.Json;
using Shared.Contracts;

namespace OrderService.Controllers;
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    public OrdersController(OrderDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var o = new Order { Id = Guid.NewGuid(), CustomerId = dto.CustomerId, Amount = dto.Amount, CreatedAt = DateTime.UtcNow };
        await _db.Orders.AddAsync(o);

        var evt = new OrderCreated(o.Id, o.CustomerId, o.Amount, o.CreatedAt);
        var payload = JsonSerializer.Serialize(evt);
        var outbox = new OutboxMessage { Id = Guid.NewGuid(), AggregateId = o.Id, Type = nameof(OrderCreated), Payload = payload, OccurredAt = DateTime.UtcNow };
        await _db.OutboxMessages.AddAsync(outbox);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = o.Id }, o);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var o = await _db.Orders.FindAsync(id);
        if (o == null) return NotFound();
        return Ok(o);
    }
}

public record CreateOrderDto(string CustomerId, decimal Amount);
