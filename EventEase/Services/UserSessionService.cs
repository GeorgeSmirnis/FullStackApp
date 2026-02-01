namespace EventEase.Services;

public class UserSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public List<int> RegisteredEventIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public interface IUserSessionService
{
    Task<UserSession> GetCurrentSessionAsync();
    Task UpdateSessionAsync(UserSession session);
    Task RegisterForEventAsync(int eventId);
    Task UnregisterFromEventAsync(int eventId);
    Task<bool> IsRegisteredForEventAsync(int eventId);
    Task<List<int>> GetRegisteredEventsAsync();
    Task ClearSessionAsync();
    Task UpdateLastActivityAsync();
}

public class UserSessionService : IUserSessionService
{
    private UserSession? _currentSession;
    private readonly object _lockObject = new();

    public UserSessionService()
    {
        // Initialize a new session
        _currentSession = new UserSession();
    }

    public Task<UserSession> GetCurrentSessionAsync()
    {
        lock (_lockObject)
        {
            if (_currentSession == null)
            {
                _currentSession = new UserSession();
            }
            return Task.FromResult(_currentSession);
        }
    }

    public Task UpdateSessionAsync(UserSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        lock (_lockObject)
        {
            session.LastActivityAt = DateTime.UtcNow;
            _currentSession = session;
            return Task.CompletedTask;
        }
    }

    public Task RegisterForEventAsync(int eventId)
    {
        if (eventId <= 0)
        {
            throw new ArgumentException("Event ID must be greater than 0.", nameof(eventId));
        }

        lock (_lockObject)
        {
            if (_currentSession == null)
            {
                _currentSession = new UserSession();
            }

            if (!_currentSession.RegisteredEventIds.Contains(eventId))
            {
                _currentSession.RegisteredEventIds.Add(eventId);
            }

            _currentSession.LastActivityAt = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }

    public Task UnregisterFromEventAsync(int eventId)
    {
        if (eventId <= 0)
        {
            throw new ArgumentException("Event ID must be greater than 0.", nameof(eventId));
        }

        lock (_lockObject)
        {
            if (_currentSession != null)
            {
                _currentSession.RegisteredEventIds.Remove(eventId);
                _currentSession.LastActivityAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }

    public Task<bool> IsRegisteredForEventAsync(int eventId)
    {
        if (eventId <= 0)
        {
            return Task.FromResult(false);
        }

        lock (_lockObject)
        {
            var isRegistered = _currentSession?.RegisteredEventIds.Contains(eventId) ?? false;
            return Task.FromResult(isRegistered);
        }
    }

    public Task<List<int>> GetRegisteredEventsAsync()
    {
        lock (_lockObject)
        {
            var registeredEvents = _currentSession?.RegisteredEventIds ?? new List<int>();
            return Task.FromResult(new List<int>(registeredEvents));
        }
    }

    public Task ClearSessionAsync()
    {
        lock (_lockObject)
        {
            _currentSession = new UserSession();
            return Task.CompletedTask;
        }
    }

    public Task UpdateLastActivityAsync()
    {
        lock (_lockObject)
        {
            if (_currentSession != null)
            {
                _currentSession.LastActivityAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }
}
