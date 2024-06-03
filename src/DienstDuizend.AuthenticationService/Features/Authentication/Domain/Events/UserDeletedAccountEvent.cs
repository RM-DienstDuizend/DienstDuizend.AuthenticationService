
namespace DienstDuizend.Events;

public class UserDeletedAccountEvent
{
    public Guid UserId { get; set; }
    public DateTime RemovedAt { get; set; } = DateTime.Now;
}