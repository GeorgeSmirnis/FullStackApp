using EventEase.Models;

namespace EventEase.Services;

public class AttendanceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int EventId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public bool CheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
}

public interface IAttendanceService
{
    Task<AttendanceRecord> RegisterAttendanceAsync(int eventId, string userName, string email);
    Task<bool> CheckInAsync(string attendanceRecordId);
    Task<List<AttendanceRecord>> GetEventAttendanceAsync(int eventId);
    Task<int> GetEventAttendanceCountAsync(int eventId);
    Task<List<AttendanceRecord>> GetUserAttendanceAsync(string email);
    Task CancelAttendanceAsync(string attendanceRecordId);
}

public class AttendanceService : IAttendanceService
{
    private List<AttendanceRecord> _attendanceRecords = new();
    private readonly object _lockObject = new();

    public Task<AttendanceRecord> RegisterAttendanceAsync(int eventId, string userName, string email)
    {
        if (eventId <= 0)
        {
            throw new ArgumentException("Event ID must be greater than 0.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        lock (_lockObject)
        {
            var record = new AttendanceRecord
            {
                EventId = eventId,
                UserName = userName ?? "Anonymous",
                UserEmail = email,
                RegistrationDate = DateTime.UtcNow,
                CheckedIn = false
            };

            _attendanceRecords.Add(record);
            return Task.FromResult(record);
        }
    }

    public Task<bool> CheckInAsync(string attendanceRecordId)
    {
        if (string.IsNullOrWhiteSpace(attendanceRecordId))
        {
            throw new ArgumentException("Attendance record ID is required.", nameof(attendanceRecordId));
        }

        lock (_lockObject)
        {
            var record = _attendanceRecords.FirstOrDefault(r => r.Id == attendanceRecordId);
            if (record == null)
            {
                return Task.FromResult(false);
            }

            record.CheckedIn = true;
            record.CheckInTime = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }

    public Task<List<AttendanceRecord>> GetEventAttendanceAsync(int eventId)
    {
        if (eventId <= 0)
        {
            return Task.FromResult(new List<AttendanceRecord>());
        }

        lock (_lockObject)
        {
            var records = _attendanceRecords
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToList();

            return Task.FromResult(records);
        }
    }

    public Task<int> GetEventAttendanceCountAsync(int eventId)
    {
        if (eventId <= 0)
        {
            return Task.FromResult(0);
        }

        lock (_lockObject)
        {
            var count = _attendanceRecords.Count(r => r.EventId == eventId && r.CheckedIn);
            return Task.FromResult(count);
        }
    }

    public Task<List<AttendanceRecord>> GetUserAttendanceAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult(new List<AttendanceRecord>());
        }

        lock (_lockObject)
        {
            var records = _attendanceRecords
                .Where(r => r.UserEmail?.Equals(email, StringComparison.OrdinalIgnoreCase) ?? false)
                .OrderByDescending(r => r.RegistrationDate)
                .ToList();

            return Task.FromResult(records);
        }
    }

    public Task CancelAttendanceAsync(string attendanceRecordId)
    {
        if (string.IsNullOrWhiteSpace(attendanceRecordId))
        {
            throw new ArgumentException("Attendance record ID is required.", nameof(attendanceRecordId));
        }

        lock (_lockObject)
        {
            var record = _attendanceRecords.FirstOrDefault(r => r.Id == attendanceRecordId);
            if (record != null)
            {
                _attendanceRecords.Remove(record);
            }

            return Task.CompletedTask;
        }
    }
}
