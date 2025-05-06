using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SlopeGuard.Services
{
    public static class MapSnapshotService
    {

        public static async Task<string> SaveSnapshotAsync(string filePath, List<Location> route)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int width = 800;
                    int height = 600;

                    using var surface = SKSurface.Create(new SKImageInfo(width, height));
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Black); // Background color

                    Console.WriteLine($"[DEBUG] Route points received: {route.Count}");

                    if (route.Count >= 2)
                    {
                        var points = NormalizeRoute(route, width, height);

                        using var paint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.Red,
                            StrokeWidth = 4,
                            IsAntialias = true
                        };

                        for (int i = 1; i < points.Count; i++)
                        {
                            canvas.DrawLine(points[i - 1], points[i], paint);
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Not enough points to draw route.");
                    }

                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory!);

                    using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
                    data.SaveTo(stream);

                    Console.WriteLine($"✅ Snapshot written to {filePath}");
                    return filePath;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error saving snapshot: {ex.Message}");
                    throw;
                }
            });
        }


        private static List<SKPoint> NormalizeRoute(List<Location> locations, int width, int height)
        {
            var minLat = locations.Min(l => l.Latitude);
            var maxLat = locations.Max(l => l.Latitude);
            var minLon = locations.Min(l => l.Longitude);
            var maxLon = locations.Max(l => l.Longitude);

            float scaleX = width / (float)(maxLon - minLon);
            float scaleY = height / (float)(maxLat - minLat);

            return locations.Select(loc =>
                new SKPoint(
                    (float)((loc.Longitude - minLon) * scaleX),
                    height - (float)((loc.Latitude - minLat) * scaleY) // flip Y to match screen
                )
            ).ToList();
        }


    }
}
