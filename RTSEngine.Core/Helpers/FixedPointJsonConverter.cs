using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RTSEngine.Core.Helpers;

public sealed class FixedPointJsonConverter : JsonConverter<FixedPoint>
{
    public override FixedPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
            {
                return new FixedPoint(intValue * 1000);
            }
            if (reader.TryGetDouble(out double doubleValue))
            {
                return FixedPoint.FromFloat((float)doubleValue);
            }
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                return FixedPoint.FromFloat(floatValue);
            }
        }

        throw new JsonException($"Unable to convert JSON to FixedPoint. Token: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, FixedPoint value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToFloat());
    }
}
