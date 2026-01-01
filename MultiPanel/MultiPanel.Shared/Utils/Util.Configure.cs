using Microsoft.Extensions.Configuration;

namespace MultiPanel.Shared.Utils;

static partial class Util
{
    extension(IConfiguration configuration)
    {
        public T SafeGetConfigureValue<T>(string key)
        {
            return configuration.GetValue<T>(key) ??
                   throw new KeyNotFoundException($"\"{key}\" is a required configure key");
        }

        public T SafeGetConfigureValue<T>(string key, T defaultValue)
        {
            return configuration.GetValue<T>(key) ?? defaultValue;
        }
    }
}