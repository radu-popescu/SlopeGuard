using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SlopeGuard.Models
{
    public class SkiSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string Duration { get; set; } = "";
        public double MaxSpeed { get; set; }
        public int Ascents { get; set; }
        public int Descents { get; set; }
        public double Distance { get; set; } // in km
        public double MaxAltitude { get; set; }
        public string? MapImagePath { get; set; } // path to the session map image

    }
}
