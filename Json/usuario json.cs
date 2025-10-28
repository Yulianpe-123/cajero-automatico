using System.Text.Json;
using CajeroApp.models;

namespace CajeroApp.Repos
{
    public class UsuarioRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public UsuarioRepo(string path)
        {
            _path = path;
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "[]");
            }
        }

        private List<Usuario> LeerTodo()
        {
            var raw = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<List<Usuario>>(raw, _opts);
            return data ?? new List<Usuario>();
        }

        private void GuardarTodo(List<Usuario> lista)
        {
            var raw = JsonSerializer.Serialize(lista, _opts);
            File.WriteAllText(_path, raw);
        }

        public List<Usuario> ObtenerTodos()
        {
            return LeerTodo();
        }

        public Usuario? ObtenerPorCuenta(string numeroCuenta)
        {
            return LeerTodo().FirstOrDefault(u => u.NumeroCuenta == numeroCuenta);
        }

        public void Agregar(Usuario u)
        {
            var lista = LeerTodo();
            lista.Add(u);
            GuardarTodo(lista);
        }

        public void Actualizar(Usuario u)
        {
            var lista = LeerTodo();
            var idx = lista.FindIndex(x => x.NumeroCuenta == u.NumeroCuenta);
            if (idx >= 0)
            {
                lista[idx] = u;
                GuardarTodo(lista);
            }
        }
    }
}