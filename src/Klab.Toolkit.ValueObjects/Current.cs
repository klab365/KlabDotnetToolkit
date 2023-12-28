namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// Valueobject which represent a current
/// </summary>
public record Current
{
    /// <summary>
    /// Represents 0 Ampere.
    /// </summary>
    public static readonly Current Zero = Create(0.0);

    /// <summary>
    /// Value of the current.
    /// </summary>
    public double Ampere { get; }

    /// <summary>
    /// Create a current in ampere
    /// </summary>
    /// <param name="ampere"></param>
    /// <returns></returns>
    public static Current Create(double ampere)
    {
        return new Current(ampere);
    }

    /// <summary>
    /// Create a current from milliampere
    /// </summary>
    /// <param name="milliAmpere"></param>
    /// <returns></returns>
    public static Current FromMilliAmpere(double milliAmpere)
    {
        return new Current(milliAmpere / 1000);
    }

    /// <summary>
    /// Ampere in milliampere
    /// </summary>
    public double MilliAmpere => Ampere * 1000;

    private Current(double ampere)
    {
        Ampere = ampere;
    }
}
