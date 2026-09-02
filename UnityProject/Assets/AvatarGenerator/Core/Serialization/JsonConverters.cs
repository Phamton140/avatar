using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Serialization
{
    public class ParameterIntentConverter : JsonConverter<ParameterIntent>
    {
        public override void WriteJson(JsonWriter writer, ParameterIntent value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("space");
            writer.WriteValue(value.Space.ToString());

            if (value.Value.HasValue)
            {
                writer.WritePropertyName("value");
                writer.WriteValue(value.Value.Value);
            }

            if (!string.IsNullOrEmpty(value.Expression))
            {
                writer.WritePropertyName("expression");
                writer.WriteValue(value.Expression);
            }

            writer.WritePropertyName("state");
            writer.WriteValue(value.State.ToString());

            writer.WriteEndObject();
        }

        public override ParameterIntent ReadJson(JsonReader reader, Type objectType, ParameterIntent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var intent = ParameterIntent.Auto();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName) continue;

                string prop = reader.Value.ToString();
                reader.Read();

                switch (prop)
                {
                    case "space":
                        Enum.TryParse(reader.Value.ToString(), out intent.Space);
                        break;
                    case "value":
                        intent.Value = Convert.ToSingle(reader.Value);
                        break;
                    case "expression":
                        intent.Expression = reader.Value.ToString();
                        break;
                    case "state":
                        Enum.TryParse(reader.Value.ToString(), out intent.State);
                        break;
                }
            }

            return intent;
        }
    }

    public class HashSetConverter : JsonConverter<HashSet<string>>
    {
        public override void WriteJson(JsonWriter writer, HashSet<string> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteValue(item);
            }
            writer.WriteEndArray();
        }

        public override HashSet<string> ReadJson(JsonReader reader, Type objectType, HashSet<string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var set = new HashSet<string>();
            if (reader.TokenType == JsonToken.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.String)
                        set.Add(reader.Value.ToString());
                }
            }
            return set;
        }
    }
}