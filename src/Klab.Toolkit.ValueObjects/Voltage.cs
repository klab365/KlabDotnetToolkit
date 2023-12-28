namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// Represents a voltage.
/// </summary>
public record Voltage
{
    /// <summary>
    /// Value of the voltage.
    /// </summary>
    public double Volts { get; }

    /// <summary>
    /// Get the voltage in millivolt.
    /// </summary>
    public double Millivolts => Volts * 1000;

    /// <summary>
    /// Get the voltage in microvolt.
    /// </summary>
    public double Microvolts => Volts * 1000000;

    /// <summary>
    /// Get the voltage in kilovolt.
    /// </summary>
    public double Kilovolts => Volts / 1000;

    /// <summary>
    /// Get the voltage in megavolt.
    /// </summary>
    public double Megavolts => Volts / 1000000;

    /// <summary>
    /// Create a voltage. in volt
    /// </summary>
    /// <param name="volt"></param>
    /// <returns></returns>
    public static Voltage Create(double volt)
    {
        return new Voltage(volt);
    }

    /// <summary>
    /// Create voltage from millivolt
    /// </summary>
    /// <param name="millivolt"></param>
    /// <returns></returns>
    public static Voltage FromMillivolt(double millivolt)
    {
        return new Voltage(millivolt / 1000);
    }

    /// <summary>
    /// Create Zero volt
    /// </summary>
    public static readonly Voltage Zero = Create(0.0);

    private Voltage(double volts)
    {
        Volts = volts;
    }
}
