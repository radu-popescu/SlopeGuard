using System;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace SlopeGuard.Converters
{
    public class FilePathToImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Console.WriteLine($"[Converter] Called with value: {value}");

            if (value is string path && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                Console.WriteLine($"[Converter] Loading image via stream: {path}");
                try
                {
                    return ImageSource.FromStream(() =>
                    {
                        var stream = File.OpenRead(path);
                        Console.WriteLine($"[Converter] Stream opened, length: {stream.Length} bytes");
                        return stream;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Converter] Error opening image stream: {ex}");
                }
            }
            else
            {
                Console.WriteLine($"[Converter] Invalid or missing file: {value}");
            }

            return null;
        }



        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
