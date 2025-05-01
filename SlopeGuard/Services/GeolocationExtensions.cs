using Microsoft.Maui.Devices.Sensors;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SlopeGuard.Services
{
    public static class GeolocationExtensions
    {
        public static async IAsyncEnumerable<Location> GetLocationUpdatesFallback(
            GeolocationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                    yield return location;

                await Task.Delay(1000, cancellationToken); // 1-second interval
            }
        }
    }
}
