namespace CajeroApp.models
{
    public class Transaccion
    {
        public string NumeroCuenta { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = ""; // "DEPOSITO", "RETIRO", "TRANSFERENCIA", "ADMIN-AJUSTE"
        public decimal Monto { get; set; }

        // Para transferencias / ajustes
        public string? CuentaDestino { get; set; }
    }
}