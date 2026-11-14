using AlertOne.Core.Interfaces;

namespace AlertOne.Shared.Blazor.States;

/// <summary>
/// State for storing and managing in-app notifications
/// </summary>
public sealed class NotificationState
{
    /// <summary>
    /// The notifications received, newest first.
    /// </summary>
    private readonly List<NotificationItem> _notifications = [];
    /// <summary>
    /// The maximum number of notifications kept.
    /// </summary>
    private readonly int _maxNotifications = 50;

    /// <summary>
    /// Event raised when notifications change
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets all notifications
    /// </summary>
    public IReadOnlyList<NotificationItem> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Gets unread notification count
    /// </summary>
    public int UnreadCount => _notifications.Count(n => !n.IsRead);

    /// <summary>
    /// Gets whether there are any unread notifications
    /// </summary>
    public bool HasUnread => UnreadCount > 0;

    /// <summary>
    /// Adds a new notification
    /// </summary>
    public void Add(string? title, string? body, IDictionary<string, string>? data)
    {
        var notification = new NotificationItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = title ?? "Notification",
            Body = body,
            Data = data,
            ReceivedAt = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Insert(0, notification);

        // Keep only the most recent notifications
        while (_notifications.Count > _maxNotifications)
        {
            _notifications.RemoveAt(_notifications.Count - 1);
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Adds a notification from event args
    /// </summary>
    public void Add(PushNotificationReceivedEventArgs e)
    {
        Add(e.Title, e.Body, e.Data);
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    public void MarkAsRead(string id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);

        if (notification is not null)
        {
            notification.IsRead = true;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Marks all notifications as read
    /// </summary>
    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications.Where(n => !n.IsRead))
        {
            notification.IsRead = true;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all notifications
    /// </summary>
    public void Clear()
    {
        _notifications.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Removes a specific notification
    /// </summary>
    public void Remove(string id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);

        if (notification is not null)
        {
            _notifications.Remove(notification);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Notifies subscribers that the notification list changed.
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>
/// Represents a single notification item
/// </summary>
public sealed class NotificationItem
{
    /// <summary>
    /// Gets the notification identifier.
    /// </summary>
    public required string Id { get; init; }
    /// <summary>
    /// Gets the notification title.
    /// </summary>
    public required string Title { get; init; }
    /// <summary>
    /// Gets the notification body.
    /// </summary>
    public string? Body { get; init; }
    /// <summary>
    /// Gets the additional data payload.
    /// </summary>
    public IDictionary<string, string>? Data { get; init; }
    /// <summary>
    /// Gets the UTC time the notification was received.
    /// </summary>
    public DateTime ReceivedAt { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the notification was read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets the alert ID from the data if present
    /// </summary>
    public string? AlertId => Data?.TryGetValue("alertId", out var id) == true ? id : null;

    /// <summary>
    /// Gets the severity from the data if present
    /// </summary>
    public string? Severity => Data?.TryGetValue("severity", out var severity) == true ? severity : null;
}
