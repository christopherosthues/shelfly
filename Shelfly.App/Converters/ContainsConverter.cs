using System.Collections.ObjectModel;
using System.Globalization;

namespace Shelfly.App.Converters;

public class ContainsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Guid itemId && parameter is ObservableCollection<Guid> collection)
        {
            return collection.Contains(itemId);
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
