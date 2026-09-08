using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Shelfly.App.Converters;

public class DateTimeNullableConverter : BaseConverterOneWay<DateTime?, string>
{
    public override string ConvertFrom(DateTime? value, CultureInfo? culture)
    {
        return value?.ToString(culture ?? CultureInfo.CurrentCulture) ?? string.Empty;
    }

    public override string DefaultConvertReturnValue { get; set; } = string.Empty;
}