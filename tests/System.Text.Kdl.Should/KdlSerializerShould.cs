namespace System.Text.Kdl.Should;

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
            Date = DateTime.Parse("2019-08-01"),
            TemperatureCelsius = 25,
            Summary = "Hot"
        };

        var kdl = KdlSerializer.Serialize(weatherForecast);
        return Verify(kdl);
    }
}
