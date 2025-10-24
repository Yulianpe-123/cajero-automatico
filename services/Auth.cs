using cajero_automatico.Models;
using cajero_automatico.Repos;
using cajero_automatico.Utils;

namespace cajero_automatico.Services;

public class AuthService
{
    private readonly UsuarioRepo _usuarios;

    public AuthService(UsuarioRepo usuarios)
    {
        _usuarios = usuarios;
    }

    public Usuario? Login(string numeroCuenta, string pin)
    {
        var u = _usuarios.ObtenerPorCuenta(numeroCuenta);
        if (u is null) return null;

        var hash = Hashing.Sha256(pin);
        return u.PinHash == hash ? u : null;
    }
}