namespace WebApplication1.Models
{
    public class Reminder
    {
      
            public int Id { get; set; }
            public string Title { get; set; }
            public DateTime ReminderDateTime { get; set; }
            public bool IsNotified { get; set; } = false;
    }
}
