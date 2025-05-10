using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlopeGuard.Models
{
    public class SkierData
    {
        // Skier's current speed in km/h or mph
        public double Speed { get; set; }

        // Skier's total distance traveled
        public double Distance { get; set; }

        // Location coordinates or description (e.g., GPS, mountain name, etc.)
        public string? Location { get; set; }

        // Total time of the session
        public TimeSpan Duration { get; set; }

        // Skier's altitude
        public double Altitude { get; set; }

        // Count of descents
        public int Descents { get; set; }

        // Count of ascents
        public int Ascents { get; set; }
    }
}
