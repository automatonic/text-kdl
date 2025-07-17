namespace Automatonic.Text.Kdl.Should;

using System.Text;

public class KdlWriterShould
{
    [Fact]
    public Task Serialize_WeatherForecast_class()
    {
        var options = new KdlWriterOptions { Indented = true };

        using var stream = new MemoryStream();
        using var writer = new KdlWriter(stream, options);

        writer.WriteStartObject();
        writer.WriteString("date", DateTimeOffset.UtcNow);
        writer.WriteNumber("temp", 42);
        writer.WriteEndObject();
        writer.Flush();

        var kdl = Encoding.UTF8.GetString(stream.ToArray());
        return Verify(kdl);
    }
}
