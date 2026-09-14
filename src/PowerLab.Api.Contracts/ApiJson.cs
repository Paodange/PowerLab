using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Creates the JSON configuration used by REST and runtime event contracts.
    /// </summary>
    public static class ApiJson
    {
        /// <summary>
        /// Creates independent serializer options for API contract JSON.
        /// </summary>
        public static JsonSerializerOptions CreateSerializerOptions()
        {
            return CreateSerializerOptions(false);
        }

        /// <summary>
        /// Creates independent serializer options and optionally enables indented output.
        /// </summary>
        public static JsonSerializerOptions CreateSerializerOptions(bool writeIndented)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(WorkflowJson.CreateSerializerOptions())
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                NumberHandling = JsonNumberHandling.Strict,
                WriteIndented = writeIndented
            };

            options.Converters.Add(new WorkflowValueDtoJsonConverter());
            return options;
        }

        /// <summary>
        /// Serializes an API contract value using a fresh API configuration.
        /// </summary>
        public static string Serialize<T>(T value, bool writeIndented = false)
        {
            return JsonSerializer.Serialize(value, CreateSerializerOptions(writeIndented));
        }

        /// <summary>
        /// Deserializes an API contract value using a fresh API configuration.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            T result = JsonSerializer.Deserialize<T>(json, CreateSerializerOptions())!;
            if (result == null)
            {
                throw new JsonException("The JSON value cannot be null for the requested API contract.");
            }

            return result;
        }
    }
}
