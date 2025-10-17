using System.Text.Json;

namespace cajero_automatico.Repos;

// Repositorio base con persistencia en JSON
public abstract class JsonRepo<T>
{
    private readonly string _path;
    private readonly JsonSerializerOptions _opt = new() { WriteIndented = true };

    protected JsonRepo(string path)
    {
        _path = path;
        if (!File.Exists(_path)) File.WriteAllText(_path, "[]");
    }

    protected List<T> Leer() =>
        JsonSerializer.Deserialize<List<T>>(File.ReadAllText(_path)) ?? new();

    protected void Guardar(List<T> items) =>
        File.WriteAllText(_path, JsonSerializer.Serialize(items, _opt));
}