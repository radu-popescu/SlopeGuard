using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class LiveSessionData
{
    [JsonProperty("Speed")]
    public double Speed { get; set; }

    [JsonProperty("Distance")]
    public double Distance { get; set; }

    [JsonProperty("Altitude")]
    public double Altitude { get; set; }

    [JsonProperty("Duration")]
    public TimeSpan Duration { get; set; }

    [JsonProperty("Ascents")]
    public int Ascents { get; set; }

    [JsonProperty("Descents")]
    public int Descents { get; set; }

    [JsonProperty("Route")]
    [JsonConverter(typeof(RouteConverter))]
    public List<LocationPoint> Route { get; set; }

    [JsonProperty("Timestamp")]
    public DateTime Timestamp { get; set; }
}

public class LocationPoint
{
    [JsonProperty("Latitude")]
    public double Latitude { get; set; }

    [JsonProperty("Longitude")]
    public double Longitude { get; set; }
}

public class RouteConverter : JsonConverter<List<LocationPoint>>
{
    public override List<LocationPoint> ReadJson(JsonReader reader, Type objectType, List<LocationPoint> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var list = new List<LocationPoint>();
        if (reader.TokenType == JsonToken.StartArray)
            list = serializer.Deserialize<List<LocationPoint>>(reader);
        else if (reader.TokenType == JsonToken.StartObject)
        {
            JObject obj = JObject.Load(reader);
            foreach (var property in obj.Properties().OrderBy(p => int.Parse(p.Name)))
                list.Add(property.Value.ToObject<LocationPoint>());
        }
        return list;
    }

    public override void WriteJson(JsonWriter writer, List<LocationPoint> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}