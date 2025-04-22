using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klab.Toolkit.Event;

/// <summary>
/// Custom JSON converter for interfaces
/// </summary>
internal sealed class EventInterfaceJsonConverter : JsonConverter<EventBase>
{
    /// <inheritdoc/>
    public override EventBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, EventBase? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                JsonSerializer.Serialize(writer, (EventBase?)null, options);
                break;
            default:
                {
                    Type type = value.GetType();
                    using JsonDocument jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(value, type, options));
                    writer.WriteStartObject();
                    writer.WriteString("$type", type.FullName);

                    foreach (JsonProperty element in jsonDocument.RootElement.EnumerateObject())
                    {
                        element.WriteTo(writer);
                    }

                    writer.WriteEndObject();
                    break;
                }
        }
    }
}
