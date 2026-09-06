using System.ComponentModel;

namespace Shelfly.App.Controls;

// ============================================================================
// IMultiSelectItemView
// ============================================================================
//
// The VIEW displayed by the DataTemplate implements this interface.
//
// The domain object does NOT implement anything.
//
// Example:
//
// <DataTemplate>
//     <views:MyItemView />
// </DataTemplate>
//
// MyItemView : ContentView, IMultiSelectItemView
//
// Its BindingContext is still the domain object.
//
// ============================================================================

public interface IMultiSelectItemView : INotifyPropertyChanged
{
    bool IsSelected { get; set; }
}