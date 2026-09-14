using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Provides the JSON settings used for plugin manifest data.
    /// </summary>
    public static class PluginManifestJson
    {
        /// <summary>
        /// Creates serializer options using the plugin manifest JSON contract.
        /// </summary>
        /// <param name="writeIndented">Whether serialized JSON should be indented.</param>
        public static JsonSerializerOptions CreateSerializerOptions(bool writeIndented = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                WriteIndented = writeIndented
            };

            return options;
        }

        /// <summary>
        /// Serializes a plugin manifest using the manifest JSON contract.
        /// </summary>
        public static string Serialize(PluginManifest manifest, bool writeIndented = false)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            return JsonSerializer.Serialize(manifest, CreateSerializerOptions(writeIndented));
        }

        /// <summary>
        /// Deserializes a plugin manifest using the manifest JSON contract.
        /// </summary>
        public static PluginManifest Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            PluginManifest? manifest = JsonSerializer.Deserialize<PluginManifest>(
                json,
                CreateSerializerOptions());
            if (manifest == null)
            {
                throw new JsonException("The manifest JSON cannot be null.");
            }

            return manifest;
        }
    }
}
