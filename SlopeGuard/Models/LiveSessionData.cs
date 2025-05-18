public class LiveSessionData
{
    public double Speed { get; set; }
    public double Distance { get; set; }
    public double Altitude { get; set; }
    public TimeSpan Duration { get; set; }
    public int Ascents { get; set; }
    public int Descents { get; set; }
    public List<LocationPoint> Route { get; set; }
    public DateTime Timestamp { get; set; }
}

public class LocationPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
