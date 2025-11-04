using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpleefResurgence.Game
{
    public class GimmickJsonConverter : JsonConverter<Gimmick>
    {
        public override Gimmick Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
            string type = jsonObject.GetProperty("Type").GetString();
            return type switch
            {
                "GimmickNone" => JsonSerializer.Deserialize<GimmickNone>(jsonObject.GetRawText(), options),
                "GimmickItem" => JsonSerializer.Deserialize<GimmickItem>(jsonObject.GetRawText(), options),
                "GimmickAccessory" => JsonSerializer.Deserialize<GimmickAccessory>(jsonObject.GetRawText(), options),
                "GimmickBuff" => JsonSerializer.Deserialize<GimmickBuff>(jsonObject.GetRawText(), options),
                "GimmickMount" => JsonSerializer.Deserialize<GimmickMount>(jsonObject.GetRawText(), options),
                "GimmickMob" => JsonSerializer.Deserialize<GimmickMob>(jsonObject.GetRawText(), options),
                _ => throw new NotSupportedException($"Gimmick type '{type}' is not supported.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Gimmick value, JsonSerializerOptions options)
        {
            var type = value.GetType().Name;
            var json = JsonSerializer.Serialize(value, value.GetType(), options);
            var jsonObject = JsonDocument.Parse(json).RootElement;

            writer.WriteStartObject();
            writer.WriteString("Type", type);

            foreach (var property in jsonObject.EnumerateObject())
                property.WriteTo(writer);

            writer.WriteEndObject();
        }
    }
}
