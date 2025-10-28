namespace CajeroApp.models
{
    public class Usuario
    {
        public string NumeroCuenta { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string PinHash { get; set; } = ""; // PIN en texto cifrado/hasheado (por ahora plain, luego Hashing)
        public bool IsAdmin { get; set; } = false;

        // Para seguridad extra (opcional, ya listo si quieres usarlo después)
        public int IntentosFallidos { get; set; } = 0;
        public bool Bloqueada { get; set; } = false;
    }
}