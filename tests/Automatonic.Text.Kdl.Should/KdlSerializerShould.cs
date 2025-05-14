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

    [Fact]
    public Task Serialize_WeatherForecast_class_with_null()
    {
        // Load the kdl file from the test_cases/input folder
        var kdl = File.ReadAllText("test_cases/input/WeatherForecast.kdl");
        var weatherForecast = KdlSerializer.Deserialize<WeatherForecast>(text);
        return Verify(kdl);
    }
}
