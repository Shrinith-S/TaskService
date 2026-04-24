using Microsoft.EntityFrameworkCore;
using TaskService.Consumer.Inbox;

namespace TaskService.Consumer.Persistence;

public class ConsumerDbContext : DbContext
{
    public ConsumerDbContext(DbContextOptions<ConsumerDbContext> options)
        : base(options)
    {
    }

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
}