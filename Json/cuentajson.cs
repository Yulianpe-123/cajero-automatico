using System.Text.Json;
using CajeroApp.models;

namespace CajeroApp.Repos
{
    public class CuentaRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public CuentaRepo(string path)
        {
            _path = path;
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "[]");
            }
        }

        private List<Cuenta> LeerTodo()
        {
            var raw = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<List<Cuenta>>(raw, _opts);
            return data ?? new List<Cuenta>();
        }

        private void GuardarTodo(List<Cuenta> lista)
        {
            var raw = JsonSerializer.Serialize(lista, _opts);
            File.WriteAllText(_path, raw);
        }

        public List<Cuenta> ObtenerTodas()
        {
            return LeerTodo();
        }

        public Cuenta? ObtenerPorNumeroCuenta(string numeroCuenta)
        {
            return LeerTodo().FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
        }

        public void Agregar(Cuenta cta)
        {
            var lista = LeerTodo();
            lista.Add(cta);
            GuardarTodo(lista);
        }

        public void Actualizar(Cuenta cta)
        {
            var lista = LeerTodo();
            var idx = lista.FindIndex(x => x.NumeroCuenta == cta.NumeroCuenta);
            if (idx >= 0)
            {
                lista[idx] = cta;
                GuardarTodo(lista);
            }
        }
    }
}