using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace PhotoBooth.Converters;

public class ByteArrayToBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            return new Avalonia.Media.Imaging.Bitmap(ms);
        }
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
