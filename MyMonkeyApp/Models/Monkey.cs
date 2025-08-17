namespace MyMonkeyApp.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a monkey species or individual with metadata and location.
/// </summary>
public sealed class Monkey
{
    /// <summary>
    /// The monkey's display name (e.g. "Baboon").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Textual location / region (e.g. "Africa & Asia").
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Short description or details about the monkey.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// URL to an image representing the monkey.
    /// Stored as a string to keep JSON compatibility; can be parsed into <see cref="Uri"/>.
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Estimated population (use 0 or 1 for single named monkeys).
    /// </summary>
    public int Population { get; init; }

    /// <summary>
    /// Latitude of the monkey's representative location.
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// Longitude of the monkey's representative location.
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    /// Convenience property that groups latitude/longitude.
    /// </summary>
    [JsonIgnore]
    public GeoLocation LocationCoordinates => new(Latitude, Longitude);
}

/// <summary>
/// Simple value type for geographic coordinates.
/// </summary>
public readonly record struct GeoLocation(double Latitude, double Longitude)
{
    /// <summary>
    /// Returns coordinates formatted to 6 decimal places.
    /// </summary>
    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}
