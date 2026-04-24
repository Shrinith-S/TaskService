namespace TaskService.Consumer.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }            // MessageId
    public DateTime ReceivedAt { get; set; }
}