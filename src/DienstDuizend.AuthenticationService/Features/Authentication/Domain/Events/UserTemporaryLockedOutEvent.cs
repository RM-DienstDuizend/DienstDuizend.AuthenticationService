namespace DienstDuizend.Events;

public class UserTemporaryLockedOutEvent
{
    public Guid UserId { get; set; }
    public string EmailAddress { get; set; }
    public string LockoutRemovalKey { get; set; }
}