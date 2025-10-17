namespace cajero_automatico.Models;

public class Usuario
{
    public string NumeroCuenta { get; set; } = "";
    public string PinHash { get; set; } = "";
    public string Nombre { get; set; } = "";
}