using System.Text.Json;

namespace ConverterApplication.Tests.Tools;

public static class TestsHelper
{
    public static T Deserialize<T>(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found at path: {path}");
        }

        var json = File.ReadAllText(path);
        var result = JsonSerializer.Deserialize<T>(json);
        
        if (result == null)
        {
            throw new InvalidOperationException($"Failed to deserialize file at path: {path}");
        }

        return result;
    }
}