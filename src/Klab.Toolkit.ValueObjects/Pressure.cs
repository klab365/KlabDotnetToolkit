﻿namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// This record represents a pressure value in pascal
/// </summary>
public record Pressure
{
    /// <summary>
    /// Represents a constant value for converting Pascal to millibar.
    /// </summary>
    public const int PASCAL_TO_MBAR = 100;

    /// <summary>
    /// Represents the conversion factor from Pascal to Bar.
    /// </summary>
    public const int PASCAL_TO_BAR = 100_000;

    /// <summary>
    /// Pressure in pascal
    /// </summary>
    public double Pascal { get; }

    /// <summary>
    /// Create a pressure value from mBar
    /// </summary>
    /// <param name="mBar"></param>
    /// <returns></returns>
    public static Pressure FromMilliBar(double mBar)
    {
        return new Pressure(mBar * PASCAL_TO_MBAR);
    }

    /// <summary>
    /// Create a pressure value from bar
    /// </summary>
    /// <param name="bar"></param>
    /// <returns></returns>
    public static Pressure FromBar(double bar)
    {
        return new Pressure(bar * PASCAL_TO_BAR);
    }

    /// <summary>
    /// Create a pressure value from pascal
    /// </summary>
    /// <param name="pascal"></param>
    /// <returns></returns>
    public static Pressure FromPascal(double pascal)
    {
        return new Pressure(pascal);
    }

    /// <summary>
    /// Get the Value in mBar
    /// </summary>
    public double MilliBar => Pascal / PASCAL_TO_MBAR;

    /// <summary>
    /// Get the value in bar
    /// </summary>
    public double Bar => Pascal / PASCAL_TO_BAR;

    /// <summary>
    /// private constructor
    /// </summary>
    /// <param name="pascal"></param>
    private Pressure(double pascal)
    {
        Pascal = pascal;
    }
}
