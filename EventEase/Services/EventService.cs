using EventEase.Models;

namespace EventEase.Services;

public interface IEventService
{
    Task<List<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(int id);
    Task<bool> IsValidEventIdAsync(int id);
    Task<int> GetEventCountAsync();
}

public class EventService : IEventService
{
    private List<Event>? _events;
    private readonly object _lockObject = new();

    public EventService()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        _events = new List<Event>
        {
            new Event
            {
                Id = 1,
                Name = "Annual Tech Conference 2026",
                Date = new DateTime(2026, 03, 15),
                Location = "San Francisco Convention Center",
                Description = "Join industry leaders for a day of innovation, networking, and cutting-edge presentations. This comprehensive conference will feature keynote speakers from leading tech companies, interactive workshops, and networking sessions designed to foster connections and share insights on the future of technology.",
                MaxAttendees = 500,
                RegisteredAttendees = 342,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=800&h=400&fit=crop"
            },
            new Event
            {
                Id = 2,
                Name = "Networking Mixer for Professionals",
                Date = new DateTime(2026, 02, 20),
                Location = "Downtown Marriott, New York",
                Description = "Casual networking event with light refreshments and music. Perfect for making new connections in a relaxed environment. This event is designed for professionals across all industries to connect, share experiences, and explore potential collaborations.",
                MaxAttendees = 200,
                RegisteredAttendees = 128,
                ImageUrl = "https://images.unsplash.com/photo-1540575467063-178f50002c4b?w=800&h=400&fit=crop"
            },
            new Event
            {
                Id = 3,
                Name = "Web Development Workshop",
                Date = new DateTime(2026, 02, 28),
                Location = "Tech Hub, Seattle",
                Description = "Hands-on workshop covering the latest in web development frameworks and best practices. Learn from expert instructors about modern web technologies, responsive design, performance optimization, and security best practices.",
                MaxAttendees = 50,
                RegisteredAttendees = 47,
                ImageUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800&h=400&fit=crop"
            },
            new Event
            {
                Id = 4,
                Name = "Corporate Team Building Event",
                Date = new DateTime(2026, 04, 10),
                Location = "Adventure Park, Boston",
                Description = "A fun and engaging team building experience with outdoor activities and team competitions. Strengthen team bonds through collaborative activities and friendly competitions in a beautiful outdoor setting.",
                MaxAttendees = 300,
                RegisteredAttendees = 215,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=800&h=400&fit=crop"
            },
            new Event
            {
                Id = 5,
                Name = "Women in Tech Summit",
                Date = new DateTime(2026, 03, 08),
                Location = "Austin Convention Center, Texas",
                Description = "Empowering women in technology with keynotes, panels, and networking opportunities. This summit celebrates women in tech and provides a platform for sharing experiences, discussing challenges, and building a supportive community.",
                MaxAttendees = 400,
                RegisteredAttendees = 356,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?w=800&h=400&fit=crop"
            },
            new Event
            {
                Id = 6,
                Name = "Product Launch Celebration",
                Date = new DateTime(2026, 05, 01),
                Location = "Innovation Hub, Chicago",
                Description = "Celebrate our latest product launch with exclusive previews and special announcements. Be among the first to experience our newest innovation and connect with the team behind it.",
                MaxAttendees = 250,
                RegisteredAttendees = 0,
                ImageUrl = "https://images.unsplash.com/photo-1540575467063-178f50002c4b?w=800&h=400&fit=crop"
            }
        };
    }

    public Task<List<Event>> GetAllEventsAsync()
    {
        lock (_lockObject)
        {
            // Return a copy to prevent external modifications
            return Task.FromResult(new List<Event>(_events ?? new List<Event>()));
        }
    }

    public Task<Event?> GetEventByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Task.FromResult<Event?>(null);
        }

        lock (_lockObject)
        {
            var evt = _events?.FirstOrDefault(e => e.Id == id);
            // Return a copy to prevent external modifications
            return Task.FromResult(evt != null ? CopyEvent(evt) : null);
        }
    }

    public Task<bool> IsValidEventIdAsync(int id)
    {
        if (id <= 0)
        {
            return Task.FromResult(false);
        }

        lock (_lockObject)
        {
            return Task.FromResult(_events?.Any(e => e.Id == id) ?? false);
        }
    }

    public Task<int> GetEventCountAsync()
    {
        lock (_lockObject)
        {
            return Task.FromResult(_events?.Count ?? 0);
        }
    }

    private Event CopyEvent(Event evt)
    {
        return new Event
        {
            Id = evt.Id,
            Name = evt.Name ?? string.Empty,
            Date = evt.Date,
            Location = evt.Location ?? string.Empty,
            Description = evt.Description ?? string.Empty,
            MaxAttendees = evt.MaxAttendees,
            RegisteredAttendees = evt.RegisteredAttendees,
            ImageUrl = evt.ImageUrl ?? string.Empty
        };
    }
}
