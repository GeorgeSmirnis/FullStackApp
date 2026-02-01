namespace EventEase.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxAttendees { get; set; }
    public int RegisteredAttendees { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
