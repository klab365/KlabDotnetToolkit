namespace Klab.Toolkit.Messaging;

/// <summary>
/// Provides an explicit JSON representation for messaging recordings.
/// </summary>
public interface IRecordable
{
    /// <summary>
    /// Gets the JSON representation to include in a recording.
    /// </summary>
    string ToRecordingDataJson();
}
