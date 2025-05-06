using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace SlopeGuard.Services
{
    public static class MapSnapshotService
    {
        public static async Task<string> SaveSnapshotAsync(string filePath, List<Location> route)
        {
            try
            {
                int width = 800;
                int height = 600;

                using var surface = SKSurface.Create(new SKImageInfo(width, height));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Black);

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

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                if (data == null)
                {
                    Console.WriteLine("❌ Failed to encode image.");
                    return filePath;
                }

                await using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
                data.SaveTo(stream);

                Console.WriteLine($"✅ Snapshot written to {filePath}, bytes: {data.Size}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in SaveSnapshotAsync: {ex.Message}");
                throw;
            }
        }






        private static List<SKPoint> NormalizeRoute(List<Location> locations, int width, int height)
        {
            var minLat = locations.Min(l => l.Latitude);
            var maxLat = locations.Max(l => l.Latitude);
            var minLon = locations.Min(l => l.Longitude);
            var maxLon = locations.Max(l => l.Longitude);

            if (Math.Abs(maxLat - minLat) < 0.0001) maxLat += 0.0001;
            if (Math.Abs(maxLon - minLon) < 0.0001) maxLon += 0.0001;

            float scaleX = width / (float)(maxLon - minLon);
            float scaleY = height / (float)(maxLat - minLat);

            var points = locations.Select(loc =>
                new SKPoint(
                    (float)((loc.Longitude - minLon) * scaleX),
                    height - (float)((loc.Latitude - minLat) * scaleY)
                )).ToList();

            for (int i = 0; i < points.Count; i++)
            {
                Console.WriteLine($"[DEBUG] Normalized point: {points[i].X}, {points[i].Y}");
            }

            return points;
        }

    }
}
