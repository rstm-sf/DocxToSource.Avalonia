using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DocxToSource.Avalonia.Converters;

public class GridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isOpenXmlElement || !targetType.IsAssignableTo(typeof(GridLength)))
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        if (!isOpenXmlElement)
            return new GridLength(0);

        var convertParameter = double.Parse(parameter as string ?? "0.0", NumberStyles.Float, CultureInfo.InvariantCulture);
        return convertParameter > 0
            ? new GridLength(convertParameter)
            : GridLength.Star;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
