using Microsoft.EntityFrameworkCore;
namespace PaymentService.Data;
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> opts) : base(opts) { }
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>().ToTable("inbox_messages");
        modelBuilder.Entity<InboxMessage>().HasIndex(i => new { i.MessageId }).IsUnique(false);
    }
}
