using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedPal.API.Serialization
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                return DateOnly.ParseExact(s, Format);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                int year = 0, month = 0, day = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject) break;
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var prop = reader.GetString();
                        reader.Read();
                        if (string.Equals(prop, "year", StringComparison.OrdinalIgnoreCase)) year = reader.GetInt32();
                        else if (string.Equals(prop, "month", StringComparison.OrdinalIgnoreCase)) month = reader.GetInt32();
                        else if (string.Equals(prop, "day", StringComparison.OrdinalIgnoreCase)) day = reader.GetInt32();
                    }
                }
                return new DateOnly(year, month, day);
            }

            throw new JsonException("Unsupported JSON token for DateOnly");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format));
    }
}