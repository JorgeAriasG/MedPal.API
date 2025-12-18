using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedPal.API.Serialization
{
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                return TimeOnly.ParseExact(s, Format);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                int hour = 0, minute = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject) break;
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var prop = reader.GetString();
                        reader.Read();
                        if (string.Equals(prop, "hour", StringComparison.OrdinalIgnoreCase))
                            hour = reader.GetInt32();
                        else if (string.Equals(prop, "minute", StringComparison.OrdinalIgnoreCase))
                            minute = reader.GetInt32();
                    }
                }
                return new TimeOnly(hour, minute);
            }

            throw new JsonException("Unsupported JSON token for TimeOnly");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}