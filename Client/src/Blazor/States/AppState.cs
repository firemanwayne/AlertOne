using AlertOne.Core.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlertOne.Shared.Blazor.States;

/// <summary>
/// Global application state with reactive property change notifications
/// </summary>
public sealed class AppState : INotifyPropertyChanged
{
    /// <summary>
    /// Backing field for the API base URL.
    /// </summary>
    private string _apiBaseUrl = "https://localhost:7071";
    /// <summary>
    /// Backing field for the access token.
    /// </summary>
    private string? _accessToken;
    /// <summary>
    /// Backing field for the signed-in user.
    /// </summary>
    private UserProfile? _currentUser;
    /// <summary>
    /// Backing field for the authenticated flag.
    /// </summary>
    private bool _isAuthenticated;
    /// <summary>
    /// Backing field for the loading flag.
    /// </summary>
    private bool _isLoading;

    /// <summary>
    /// Gets or sets the API base URL
    /// </summary>
    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => SetProperty(ref _apiBaseUrl, value);
    }

    /// <summary>
    /// Gets or sets the current access token
    /// </summary>
    public string? AccessToken
    {
        get => _accessToken;
        set => SetProperty(ref _accessToken, value);
    }

    /// <summary>
    /// Gets or sets the current user profile
    /// </summary>
    public UserProfile? CurrentUser
    {
        get => _currentUser;
        set
        {
            if (SetProperty(ref _currentUser, value))
            {
                IsAuthenticated = value is not null;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a loading operation is in progress
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Clears the authentication state
    /// </summary>
    public void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        IsAuthenticated = false;
    }

    /// <summary>
    /// Event raised when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for a property.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets a field and raises <see cref="PropertyChanged"/> when the value changed; returns whether it changed.
    /// </summary>
    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
