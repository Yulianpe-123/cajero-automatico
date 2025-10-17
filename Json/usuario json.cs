using cajero_automatico.Models;

namespace cajero_automatico.Repos;

public class UsuarioRepo : JsonRepo<Usuario>
{
    public UsuarioRepo(string path) : base(path) { }

    public List<Usuario> ObtenerTodos() => Leer();

    public Usuario? ObtenerPorCuenta(string cuenta) =>
        Leer().FirstOrDefault(u => u.NumeroCuenta == cuenta);

    public void Agregar(Usuario u)
    {
        var lista = Leer();
        lista.Add(u);
        Guardar(lista);
    }
}