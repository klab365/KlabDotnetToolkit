namespace Klab.Toolkit.Common.ValueObjects;

/// <summary>
/// This record represents a pressure value in pascal
/// </summary>
public record Pressure
{
    private const int PASCAL_TO_MBAR = 100;
    private const int PASCAL_TO_BAR = 100_000;

    /// <summary>
    /// Pressure in pascal
    /// </summary>
    public double Pascal { get; init; }

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
    ///
    /// </summary>
    /// <param name="bar"></param>
    /// <returns></returns>
    public static Pressure FromBar(double bar)
    {
        return new Pressure(bar * PASCAL_TO_BAR);
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
    /// <param name="value"></param>
    private Pressure(double value)
    {
        Pascal = value;
    }
}
