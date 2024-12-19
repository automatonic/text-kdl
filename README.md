# System.Text.Kdl
Goal - C# support for [KDL](kdl.dev) in the style of the [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to) library.

- Compare and contrast `NewtonSoft.Json` (with which I am very familiar) to `System.Text.Json` for posterity and performance
- Use the resulting knowledge of low allocation span based parsers to create `System.Text.Kdl` primitives
- Build out additional features as relevant/appropriate

# Must Have
- `KdlReader` - Fast forward-only parser/reader (analogous to `Utf8JsonReader`)
- Full spec compatibility (KDL 2.0) verified via unit test converage and the the well-defined test cases
- `KdlWriter` - Low-level primitive writer (analogous to `Utf8JsonWriter`)

# Should Have
- `KdlDocument` - In memory DOM for random access (analogous to `JsonDocument`)
- `KdlSerializer` - Tool for serializing/deserializing KDL to/from objects in memory (analogous to `JsonConvert` in NewtonSoft, or `JsonSerializer` in System.Text.Json)

# May Have
- KDL css-like style selectors implementation using `KdlReader`
- Linq to objects style support (c.f. `JObject`, etc)
- Reflection based serialization (c.f. `NewtonSoft.Json`, `System.Text.Json`)
- Helper types and routines to bind KDL to asp.net core as a supported/bindable content-type?
- Generation based serialization (c.f. `System.Text.Json`)

# Will Not Have
- strict Support for KDL 1.0. Going with the fact that KDL 2.0 supercedes in every way that is important.
- Backwards compatibility for older .NET or older .NET core. Simplifying by starting with dotnet 9.0 and beyond. Even a .net standard implementation seems to be no longer strictly necessary moving forward (in my reading).
