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
                Console.WriteLine($"[Converter] Loading image from: {path}");
                try
                {
                    return ImageSource.FromStream(() => File.OpenRead(path));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Converter] Error loading image: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[Converter] File not found or invalid path: {value}");
            }

            return null;
        }


        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
