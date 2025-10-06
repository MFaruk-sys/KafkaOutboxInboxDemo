using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using Serilog;
using System.Text.Json;
using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

var conn = builder.Configuration.GetConnectionString("Mssql") ??
           builder.Configuration["ConnectionStrings:Mssql"] ??
           builder.Configuration["ConnectionStrings__Mssql"] ??
           "Server=mssql_payment,1433;Database=paymentdb;User Id=sa;Password=YourStrong!Passw0rd;";

builder.Services.AddDbContext<PaymentDbContext>(opt => opt.UseSqlServer(conn));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit + Kafka consumer (simple)
var broker = builder.Configuration["Kafka:BootstrapServers"] ?? builder.Configuration["Kafka__BootstrapServers"] ?? "kafka:9092";
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    x.AddRider(r =>
    {
        r.AddConsumer<OrderCreatedConsumer>();
        r.UsingKafka((context, k) =>
        {
            k.Host(broker);
            k.TopicEndpoint<Shared.Contracts.OrderCreated>("order_events", "payment-group", e =>
            {
                e.ConfigureConsumer<OrderCreatedConsumer>(context);
            });
        });
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger(); app.UseSwaggerUI();
app.MapControllers();
app.Run();

// Consumer implementation
public class OrderCreatedConsumer : IConsumer<Shared.Contracts.OrderCreated>
{
    private readonly PaymentDbContext _db;
    public OrderCreatedConsumer(PaymentDbContext db) => _db = db;
    public async Task Consume(ConsumeContext<Shared.Contracts.OrderCreated> context)
    {
        var msg = context.Message;
        var messageId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString();

        // Inbox pattern: avoid duplicate processing using unique constraint fallback is possible
        var kafkaKey = context.Headers.Get<string>("kafka_key");
        if (await _db.InboxMessages.AnyAsync(x => x.MessageId == messageId || x.MessageKey == kafkaKey))
        {
            Log.Information("Duplicate message skipped: {MessageId}", messageId);
            return;
        }

        _db.InboxMessages.Add(new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            MessageKey = context.Headers.Get<string>("kafka_key"),
            ProcessedAt = DateTime.UtcNow,
            Payload = JsonSerializer.Serialize(msg)
        });
        await _db.SaveChangesAsync();

        Log.Information("Processed OrderCreated {OrderId} {Amount}", msg.OrderId, msg.Amount);
    }
}
