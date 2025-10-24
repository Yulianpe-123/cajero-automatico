namespace cajero_automatico.Utils;

// Utilidad para mensajes en consola con toque "humano".
public static class ConsoleUI
{
    public static void Limpiar() => Console.Clear();

    public static void Titulo(string texto)
    {
        Console.WriteLine(new string('=', texto.Length));
        Console.WriteLine(texto);
        Console.WriteLine(new string('=', texto.Length));
    }

    public static void Caja(string texto)
    {
        var linea = new string('─', texto.Length + 2);
        Console.WriteLine($"┌{linea}┐");
        Console.WriteLine($"│ {texto} │");
        Console.WriteLine($"└{linea}┘");
    }

    public static void Info(string msg) => Console.WriteLine("[i] " + msg);
    public static void Advertencia(string msg) => Console.WriteLine("[!] " + msg);
    public static void Error(string msg) => Console.WriteLine("[x] " + msg);
    public static void Ok(string msg) => Console.WriteLine("[✓] " + msg);

    public static void Continuar()
    {
        Console.Write("Presiona una tecla para continuar...");
        Console.ReadKey(true);
    }

    // Lee PIN sin mostrar caracteres
    public static string LeerOculto()
    {
        var pin = "";
        ConsoleKeyInfo k;
        while ((k = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (k.Key == ConsoleKey.Backspace && pin.Length > 0)
                pin = pin[..^1];
            else if (!char.IsControl(k.KeyChar))
                pin += k.KeyChar;
        }
        Console.WriteLine();
        return pin;
    }
}