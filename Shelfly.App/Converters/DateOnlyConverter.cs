using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Shelfly.App.Converters;

public class DateOnlyConverter : BaseConverterOneWay<DateTime?, string>
{
    public override string ConvertFrom(DateTime? value, CultureInfo? culture)
    {
        return value?.Date.ToShortDateString() ?? string.Empty;
    }

    public override string DefaultConvertReturnValue { get; set; } = string.Empty;
}