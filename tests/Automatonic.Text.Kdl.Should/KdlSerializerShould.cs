namespace Automatonic.Text.Kdl.Should;

public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string? Summary { get; set; }
}

public class KdlSerializerShould
{
    [Fact]
    public Task Serialize_WeatherForecast_class()
    {
        WeatherForecast weatherForecast = new()
        {
            Date = new DateTimeOffset(2000, 11, 22, 0, 0, 0, TimeSpan.Zero),
            TemperatureCelsius = 25,
            Summary = "Hot"
        };

        var kdl = KdlSerializer.Serialize(weatherForecast);
        return Verify(kdl);
    }
}
