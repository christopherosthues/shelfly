using System.Globalization;
using CommunityToolkit.Maui.Converters;
using Shelfly.App.Data.Entities;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.Converters;

public class BookDateConverter : BaseConverterOneWay<BookEntity, string>
{
    public override string ConvertFrom(BookEntity value, CultureInfo? culture)
    {
        DateTime displayDate = value.LastModifiedAt ?? value.CreatedAt;
        TimeSpan timeSpan = DateTime.Now - displayDate.Date;

        return timeSpan.TotalDays switch
        {
            < 1 => AppResources.CommonDateToday + " " + displayDate.ToString(AppResources.CommonDateTimeFormatToday),
            < 2 => AppResources.CommonDateYesterday,
            < 7 => string.Format(AppResources.CommonDateDaysAgo, (int)timeSpan.TotalDays),
            < 30 => string.Format(AppResources.CommonDateWeeksAgo, (int)(timeSpan.TotalDays / 7)),
            < 365 => string.Format(AppResources.CommonDateMonthsAgo, (int)(timeSpan.TotalDays / 30)),
            _ => string.Format(AppResources.CommonDateYearsAgo, (int)(timeSpan.TotalDays / 365))
        };
    }

    public override string DefaultConvertReturnValue { get; set; } = string.Empty;
}