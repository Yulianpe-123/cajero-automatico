using cajero_automatico.Models;

namespace cajero_automatico.Repos;

public class TransaccionRepo : JsonRepo<Transaccion>
{
    public TransaccionRepo(string path) : base(path) { }

    public List<Transaccion> ObtenerTodos() => Leer();

    public List<Transaccion> ObtenerPorCuenta(string cuenta) =>
        Leer().Where(t => t.NumeroCuenta == cuenta).ToList();

    public void Agregar(Transaccion t)
    {
        var lista = Leer();
        lista.Add(t);
        Guardar(lista);
    }
}