using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public record OrderCreated(Guid OrderId, string CustomerId, decimal Amount, DateTime CreatedAt)
    {
        public string GetPayload()
        {
            // Serialize as JsonElement first
            var jsonElement = JsonSerializer.SerializeToElement(this);
            // Convert JsonElement to raw JSON string
            return jsonElement.GetRawText();
        }
    }
}
