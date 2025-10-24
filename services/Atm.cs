using cajero_automatico.Models;
using cajero_automatico.Repos;

namespace cajero_automatico.Services;

public class AtmService
{
    private readonly CuentaRepo _cuentas;
    private readonly TransaccionRepo _trans;

    public AtmService(CuentaRepo cuentas, TransaccionRepo trans)
    {
        _cuentas = cuentas;
        _trans = trans;
    }

    public decimal ObtenerSaldo(string numeroCuenta)
    {
        var c = _cuentas.ObtenerPorCuenta(numeroCuenta)
                ?? throw new InvalidOperationException("La cuenta no existe.");
        return c.Saldo;
    }

    public void Depositar(string numeroCuenta, decimal monto)
    {
        if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a cero.");
        var c = _cuentas.ObtenerPorCuenta(numeroCuenta)
                ?? throw new InvalidOperationException("La cuenta no existe.");
        c.Saldo += monto;
        _cuentas.GuardarOCrear(c);

        _trans.Agregar(new Transaccion
        {
            NumeroCuenta = numeroCuenta,
            Tipo = TipoTransaccion.Deposito,
            Monto = monto,
            Detalle = "Depósito en cuenta"
        });
    }

    public void Retirar(string numeroCuenta, decimal monto)
    {
        if (monto <= 0) throw new InvalidOperationException("El monto debe ser mayor a cero.");
        var c = _cuentas.ObtenerPorCuenta(numeroCuenta)
                ?? throw new InvalidOperationException("La cuenta no existe.");
        if (c.Saldo < monto) throw new InvalidOperationException("Saldo insuficiente.");
        c.Saldo -= monto;
        _cuentas.GuardarOCrear(c);

        _trans.Agregar(new Transaccion
        {
            NumeroCuenta = numeroCuenta,
            Tipo = TipoTransaccion.Retiro,
            Monto = monto,
            Detalle = "Retiro en cuenta"
        });
    }

    public void Transferir(string origen, string destino, decimal monto)
    {
        if (string.IsNullOrWhiteSpace(destino))
            throw new InvalidOperationException("La cuenta destinada es requerida.");
        if (origen == destino)
            throw new InvalidOperationException("No puedes transferir a la misma cuenta.");
        if (monto <= 0)
            throw new InvalidOperationException("El monto debe ser mayor a cero.");

        var cOrigen = _cuentas.ObtenerPorCuenta(origen)
                       ?? throw new InvalidOperationException("Cuenta de origen no existe.");
        var cDestino = _cuentas.ObtenerPorCuenta(destino)
                       ?? throw new InvalidOperationException("Cuenta destinada no existe.");
        if (cOrigen.Saldo < monto) throw new InvalidOperationException("Saldo insuficiente.");

        cOrigen.Saldo -= monto;
        cDestino.Saldo += monto;
        _cuentas.GuardarOCrear(cOrigen);
        _cuentas.GuardarOCrear(cDestino);

        _trans.Agregar(new Transaccion
        {
            NumeroCuenta = origen,
            Tipo = TipoTransaccion.TransferenciaSalida,
            Monto = monto,
            Detalle = "Transferencia a {destino}"
        });
        _trans.Agregar(new Transaccion
        {
            NumeroCuenta = destino,
            Tipo = TipoTransaccion.TransferenciaEntrada,
            Monto = monto,
            Detalle = "Transferencia desde {origen}"
        });
    }

    public List<Transaccion> Historial(string numeroCuenta) =>
        _trans.ObtenerPorCuenta(numeroCuenta);
}