using CajeroApp.models;
using CajeroApp.Repos;
using CajeroApp.Utils;

namespace CajeroApp.Services
{
    public class AtmService
    {
        private readonly CuentaRepo _cuentaRepo;
        private readonly TransaccionRepo _transRepo;
        private readonly UsuarioRepo _usuarioRepo;

        public AtmService(CuentaRepo cuentaRepo, TransaccionRepo transRepo, UsuarioRepo usuarioRepo)
        {
            _cuentaRepo = cuentaRepo;
            _transRepo = transRepo;
            _usuarioRepo = usuarioRepo;
        }

        public bool Depositar(string numeroCuenta, decimal monto, string? adminQueHaceEsto = null)
        {
            var cuenta = _cuentaRepo.ObtenerPorNumeroCuenta(numeroCuenta);
            if (cuenta == null) return false;

            cuenta.Saldo += monto;
            _cuentaRepo.Actualizar(cuenta);

            _transRepo.Agregar(new Transaccion
            {
                NumeroCuenta = numeroCuenta,
                Fecha = DateTime.Now,
                Tipo = adminQueHaceEsto == null ? "DEPOSITO" : "ADMIN-AJUSTE",
                Monto = monto,
                CuentaDestino = adminQueHaceEsto
            });

            return true;
        }

        public bool Retirar(string numeroCuenta, decimal monto)
        {
            var cuenta = _cuentaRepo.ObtenerPorNumeroCuenta(numeroCuenta);
            if (cuenta == null) return false;

            // validación de saldo
            if (cuenta.Saldo < monto) return false;

            // validación de límite por operación (ejemplo 2000)
            if (monto > 2000m) return false;

            cuenta.Saldo -= monto;
            _cuentaRepo.Actualizar(cuenta);

            _transRepo.Agregar(new Transaccion
            {
                NumeroCuenta = numeroCuenta,
                Fecha = DateTime.Now,
                Tipo = "RETIRO",
                Monto = monto
            });

            return true;
        }

        public bool Transferir(string cuentaOrigen, string cuentaDestino, decimal monto)
        {
            if (cuentaOrigen == cuentaDestino) return false;

            var origen = _cuentaRepo.ObtenerPorNumeroCuenta(cuentaOrigen);
            var destino = _cuentaRepo.ObtenerPorNumeroCuenta(cuentaDestino);

            if (origen == null || destino == null) return false;
            if (origen.Saldo < monto) return false;

            origen.Saldo -= monto;
            _cuentaRepo.Actualizar(origen);

            destino.Saldo += monto;
            _cuentaRepo.Actualizar(destino);

            _transRepo.Agregar(new Transaccion
            {
                NumeroCuenta = cuentaOrigen,
                Fecha = DateTime.Now,
                Tipo = "TRANSFERENCIA",
                Monto = monto,
                CuentaDestino = cuentaDestino
            });

            _transRepo.Agregar(new Transaccion
            {
                NumeroCuenta = cuentaDestino,
                Fecha = DateTime.Now,
                Tipo = "TRANSFERENCIA",
                Monto = monto,
                CuentaDestino = cuentaDestino
            });

            return true;
        }

        public List<Transaccion> ObtenerHistorial(string numeroCuenta)
        {
            return _transRepo.ObtenerPorCuenta(numeroCuenta)
                             .OrderByDescending(t => t.Fecha)
                             .ToList();
        }

        // Crear cuenta (admin crea o usuario crea)
        public bool CrearCuenta(string numeroCuenta, string nombreTitular, string pinClaro, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(numeroCuenta)) return false;
            if (string.IsNullOrWhiteSpace(nombreTitular)) return false;
            if (string.IsNullOrWhiteSpace(pinClaro)) return false;

            // ¿ya existe?
            var yaU = _usuarioRepo.ObtenerPorCuenta(numeroCuenta);
            var yaC = _cuentaRepo.ObtenerPorNumeroCuenta(numeroCuenta);
            if (yaU != null || yaC != null)
            {
                return false;
            }

            var hash = Hashing.Sha256(pinClaro);

            var nuevoUsuario = new Usuario
            {
                NumeroCuenta = numeroCuenta,
                Nombre = nombreTitular,
                PinHash = hash,
                IsAdmin = isAdmin,
                IntentosFallidos = 0,
                Bloqueada = false
            };

            var nuevaCuenta = new Cuenta
            {
                NumeroCuenta = numeroCuenta,
                Saldo = 0m
            };

            _usuarioRepo.Agregar(nuevoUsuario);
            _cuentaRepo.Agregar(nuevaCuenta);

            return true;
        }

        public bool DesbloquearCuenta(string numeroCuenta)
        {
            var u = _usuarioRepo.ObtenerPorCuenta(numeroCuenta);
            if (u == null) return false;

            u.Bloqueada = false;
            u.IntentosFallidos = 0;
            _usuarioRepo.Actualizar(u);
            return true;
        }

        public List<Cuenta> ListarTodasLasCuentas()
        {
            return _cuentaRepo.ObtenerTodas()
                              .OrderBy(c => c.NumeroCuenta)
                              .ToList();
        }
    }
}