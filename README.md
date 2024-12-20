# System.Text.Kdl
C# support for [KDL](https://kdl.dev) in the style of the [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to) library.

> This is a goal / passion project for [me](https://github.com/el2iot2) personally. I will be using it
> as a self-directed exercise to expand my skills with several key SDKs in modern
> C# and to "scratch the itch" of supporting/implementing a configuration language that I find interesting / compelling. I want to:
> - Compare and contrast `NewtonSoft.Json` (with which I am very familiar) to `System.Text.Json` for posterity and performance
> - Use the resulting knowledge of low allocation span-based parsers to create `System.Text.Kdl` primitives
> - Build out additional features as relevant/appropriate

# Must Have
- `KdlReader` - Fast forward-only parser/reader (analogous to `Utf8JsonReader`)
- Full spec compatibility (KDL 2.0) verified via unit test converage and the the well-defined test cases
- NuGet Package for easy / safe consumption
- `KdlWriter` - Low-level primitive writer (analogous to `Utf8JsonWriter`)

# Should Have
- `KdlDocument` - In memory DOM for random access (analogous to `JsonDocument`)
- `KdlSerializer` - Tool for serializing/deserializing KDL to/from objects in memory (analogous to `JsonConvert` in NewtonSoft, or `JsonSerializer` in System.Text.Json)

# May Have (especially if prioritized by sponsorship)
- KDL css-like style selectors implementation using `KdlReader` for efficient results
- Linq to objects style support (c.f. `JObject`, etc)
- Reflection based serialization (c.f. `NewtonSoft.Json`, `System.Text.Json`)
- Helper types and routines to bind KDL to asp.net core as a supported/bindable content-type?
- Generation based serialization (c.f. `System.Text.Json`)

# Will Not Have (without explicit sponsorship)
- **Strict Support for KDL 1.0.** KDL 2.0 supercedes in every descernable way and supports much of what KDL 1.0 documents would have express.
- **Assumptions about the platform consuming this assembly.** Should work wherever dotnet core does.
- **Backwards compatibility** for older .NET or older .NET core. I will be simplifying by starting with dotnet 9.0 and beyond. Even a .net standard implementation seems to be no longer strictly necessary moving forward (in my reading).
