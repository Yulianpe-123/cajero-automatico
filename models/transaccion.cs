namespace cajero_automatico.Models;

public class Transaccion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string NumeroCuenta { get; set; } = "";
    public TipoTransaccion Tipo { get; set; }
    public decimal Monto { get; set; }
    public string Detalle { get; set; } = "";
}