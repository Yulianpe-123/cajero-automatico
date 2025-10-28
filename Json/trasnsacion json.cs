using System.Text.Json;
using CajeroApp.models;

namespace CajeroApp.Repos
{
    public class TransaccionRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public TransaccionRepo(string path)
        {
            _path = path;
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "[]");
            }
        }

        private List<Transaccion> LeerTodo()
        {
            var raw = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<List<Transaccion>>(raw, _opts);
            return data ?? new List<Transaccion>();
        }

        private void GuardarTodo(List<Transaccion> lista)
        {
            var raw = JsonSerializer.Serialize(lista, _opts);
            File.WriteAllText(_path, raw);
        }

        public List<Transaccion> ObtenerPorCuenta(string numeroCuenta)
        {
            return LeerTodo()
                .Where(t => t.NumeroCuenta == numeroCuenta)
                .ToList();
        }

        public void Agregar(Transaccion t)
        {
            var lista = LeerTodo();
            lista.Add(t);
            GuardarTodo(lista);
        }
    }
}