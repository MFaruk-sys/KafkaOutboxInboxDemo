namespace PaymentService.Data;
public class InboxMessage
{
    public Guid Id { get; set; }
    public string? MessageId { get; set; }
    public string? MessageKey { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Payload { get; set; } = default!;
}
