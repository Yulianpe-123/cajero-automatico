using CajeroApp.models;
using CajeroApp.Repos;
using CajeroApp.Utils;

namespace CajeroApp.Services
{
    public class AuthService
    {
        private readonly UsuarioRepo _usuarioRepo;

        public AuthService(UsuarioRepo usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public Usuario? Autenticar(string numeroCuenta, string pinClaro)
        {
            var usuario = _usuarioRepo.ObtenerPorCuenta(numeroCuenta);
            if (usuario == null) return null;

            if (usuario.Bloqueada) return null; // si quieres usar bloqueo

            var hashIngresado = Hashing.Sha256(pinClaro);

            if (hashIngresado == usuario.PinHash)
            {
                // reset de intentos fallidos al entrar bien
                usuario.IntentosFallidos = 0;
                _usuarioRepo.Actualizar(usuario);
                return usuario;
            }
            else
            {
                // intento fallido
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.Bloqueada = true;
                }
                _usuarioRepo.Actualizar(usuario);
                return null;
            }
        }
    }
}