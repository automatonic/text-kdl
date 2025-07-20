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

        var utcNow = new DateTimeOffset(2000, 11, 22, 0, 0, 0, TimeSpan.Zero);

        writer.WriteStartChildrenBlock();
        writer.WriteString("date", utcNow);
        writer.WriteNumber("temp", 42);
        writer.WriteEndChildrenBlock();
        writer.Flush();

        var kdl = Encoding.UTF8.GetString(stream.ToArray());
        return Verify(kdl);
    }

    // [Fact]
    // public Task Write_Bare_Node()
    // {
    //     var options = new KdlWriterOptions { Indented = true };

    //     using var stream = new MemoryStream();
    //     using var writer = new KdlWriter(stream, options);

    //     var utcNow = new DateTimeOffset(2000, 11, 22, 0, 0, 0, TimeSpan.Zero);

    //     writer.WriteNode("bare");
    //     writer.WriteStartChildrenBlock();
    //     writer.WriteString("date", utcNow);
    //     writer.WriteNumber("temp", 42);
    //     writer.WriteEndChildrenBlock();
    //     writer.Flush();

    //     var kdl = Encoding.UTF8.GetString(stream.ToArray());
    //     return Verify(kdl);
    // }
}
