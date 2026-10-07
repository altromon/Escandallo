using System.IO;
using System.Text.Json;

namespace Escandallo.Core;

public static class TenantPersistenceRepository
{
    public static string SerializeTenantCatalog(TenantCatalogDto catalog)
    {
        return JsonSerializer.Serialize(catalog);
    }

    public static TenantCatalogDto DeserializeTenantCatalog(string json)
    {
        return JsonSerializer.Deserialize<TenantCatalogDto>(json)!;
    }

    public static void SaveToFile(string filePath, TenantCatalogDto catalog)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(filePath, SerializeTenantCatalog(catalog));
    }

    public static TenantCatalogDto LoadFromFile(string filePath)
    {
        return DeserializeTenantCatalog(File.ReadAllText(filePath));
    }
}
