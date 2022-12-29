using System.Text.Json;

namespace KeySafe.ViewModels.Service;

public static class KsJsonSerializer
{
    public static ValueTask<T> DeserializeAsync<T>(this Stream utf8Json)
    {
        return JsonSerializer.DeserializeAsync<T>(utf8Json);
    }

    public static byte[] SerializeToUtf8Bytes<TValue>(this TValue value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value);
    }

    public static TValue Deserialize<TValue>(this string json)
    {
        return JsonSerializer.Deserialize<TValue>(json);
    }

    public static string Serialize<TValue>(this TValue value)
    {
        return JsonSerializer.Serialize(value);
    }
}