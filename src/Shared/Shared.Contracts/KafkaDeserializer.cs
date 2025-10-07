using System;
using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Shared.Contracts
{
    public class KafkaDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull) return default!;

            // Convert bytes to string
            var dataJsonString = Encoding.UTF8.GetString(data);

            // Remove extra quotes added by Debezium
            var normalizedJsonString = JsonConvert.DeserializeObject<string>(dataJsonString);

            if (string.IsNullOrEmpty(normalizedJsonString))
            {
                return default!;
            }

            // Deserialize to your actual type
            return JsonConvert.DeserializeObject<T>(normalizedJsonString)!;
        }
    }
}
