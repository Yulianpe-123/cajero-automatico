using cajero_automatico.Models;

namespace cajero_automatico.Repos;

public class CuentaRepo : JsonRepo<Cuenta>
{
    public CuentaRepo(string path) : base(path) { }

    public List<Cuenta> ObtenerTodos() => Leer();

    public Cuenta? ObtenerPorCuenta(string cuenta) =>
        Leer().FirstOrDefault(c => c.NumeroCuenta == cuenta);

    public void GuardarOCrear(Cuenta cta)
    {
        var lista = Leer();
        var i = lista.FindIndex(c => c.NumeroCuenta == cta.NumeroCuenta);
        if (i >= 0) lista[i] = cta; else lista.Add(cta);
        Guardar(lista);
    }

    public void Agregar(Cuenta cta)
    {
        var lista = Leer();
        lista.Add(cta);
        Guardar(lista);
    }
}