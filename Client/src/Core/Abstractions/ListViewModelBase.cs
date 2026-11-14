namespace AlertOne.Core.Abstractions;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// View mode base class. USE IF YOUR View model REQUIRES DROP DOWNS.
/// </summary>
public abstract class ListViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for a property.
    /// </summary>
    public void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}

/// <summary>
/// View mode base class. DO NOT USE IF YOUR View model REQUIRES DROP DOWNS.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ListViewModelBase<T> : ListViewModelBase where T : ViewModelBase
{
    /// <summary>
    /// The view models in the list.
    /// </summary>
    readonly HashSet<T> _viewModels = [];

    /// <summary>
    /// Initializes an empty list.
    /// </summary>
    protected ListViewModelBase() { }

    /// <summary>
    /// Initializes the list with existing view models.
    /// </summary>
    protected ListViewModelBase(IEnumerable<T> viewModels)
    {
        foreach (T model in viewModels)
        {
            _viewModels.Add(model);
        }

        OnPropertyChanged();
    }

    /// <summary>
    /// Gets the view models, most recently updated first.
    /// </summary>
    public HashSet<T> ViewModels => [.. _viewModels.OrderByDescending(a => a.UpdatedDate)];

    /// <summary>
    /// Adds a view model to the list.
    /// </summary>
    public void AddViewModel(T model)
    {
        if (_viewModels.Add(model))
        {
            OnPropertyChanged();
        }
        else
        {
            Console.WriteLine("DUPLICATE");
        }
    }

    /// <summary>
    /// Removes a view model from the list.
    /// </summary>
    public void RemoveViewModel(T model)
    {
        if (_viewModels.Remove(model))
        {
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Removes every view model from the list.
    /// </summary>
    public void ClearViewModels()
    {
        _viewModels.Clear();

        OnPropertyChanged();
    }
}
