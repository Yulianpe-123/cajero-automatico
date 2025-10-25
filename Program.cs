using cajero_automatico.Models;
using cajero_automatico.Repos;
using cajero_automatico.Services;
using cajero_automatico.Utils;

var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
Directory.CreateDirectory(dataDir);

// Rutas JSON
var usuariosPath = Path.Combine(dataDir, "usuarios.json");
var cuentasPath = Path.Combine(dataDir, "cuentas.json");
var transPath = Path.Combine(dataDir, "transacciones.json");

// Repos
var usuarioRepo = new UsuarioRepo(usuariosPath);
var cuentaRepo = new CuentaRepo(cuentasPath);
var transRepo = new TransaccionRepo(transPath);

// Servicios
var auth = new AuthService(usuarioRepo);
var atm = new AtmService(cuentaRepo, transRepo);

// Semilla demo
if (usuarioRepo.ObtenerTodos().Count == 0 && cuentaRepo.ObtenerTodos().Count == 0)
{
    ConsoleUI.Titulo("Inicializando datos de ejemplo");
    var demo = new Usuario
    {
        NumeroCuenta = "1001",
        Nombre = "Usuario Demo",
        PinHash = Hashing.Sha256("1234")
    };
    usuarioRepo.Agregar(demo);
    cuentaRepo.Agregar(new Cuenta { NumeroCuenta = "1001", Saldo = 1500.00m });
    ConsoleUI.Info("Cuenta demo 1001 (PIN 1234) creada.");
    ConsoleUI.Continuar();
}

// Menú principal
while (true)
{
    ConsoleUI.Limpiar();
    ConsoleUI.Caja("SIMULADOR DE CAJERO AUTOMÁTICO");

    Console.WriteLine("1) Iniciar sesión");
    Console.WriteLine("2) Crear nueva cuenta");
    Console.WriteLine("3) Salir");
    Console.Write("Opción: ");
    var op = Console.ReadLine();

    switch (op)
    {
        case "1": IniciarSesion(); break;
        case "2": CrearCuenta(); break;
        case "3":
            ConsoleUI.Ok("¡Gracias por usar el simulador!");
            return;
        default:
            ConsoleUI.Advertencia("Opción inválida.");
            ConsoleUI.Continuar();
            break;
    }
}

void IniciarSesion()
{
    ConsoleUI.Limpiar();
    ConsoleUI.Titulo("Inicio de sesión");

    Console.Write("Número de cuenta: ");
    var cuenta = Console.ReadLine()?.Trim() ?? "";

    Console.Write("PIN: ");
    var pin = ConsoleUI.LeerOculto();

    var usuario = auth.Login(cuenta, pin);
    if (usuario is null)
    {
        ConsoleUI.Error("Cuenta o PIN incorrecto.");
        ConsoleUI.Continuar();
        return;
    }

    ConsoleUI.Ok("Bienvenido, {usuario.Nombre}");
    ConsoleUI.Continuar();
    MenuUsuario(usuario.NumeroCuenta);
}

void MenuUsuario(string numeroCuenta)
{
    while (true)
    {
        ConsoleUI.Limpiar();
        ConsoleUI.Caja($"Cuenta {numeroCuenta}");
        Console.WriteLine("1) Consultar saldo");
        Console.WriteLine("2) Depositar");
        Console.WriteLine("3) Retirar");
        Console.WriteLine("4) Transferir");
        Console.WriteLine("5) Ver historial");
        Console.WriteLine("6) Cerrar sesión");
        Console.Write("Opción: ");
        var op = Console.ReadLine();

        try
        {
            switch (op)
            {
                case "1":
                    var saldo = atm.ObtenerSaldo(numeroCuenta);
                    ConsoleUI.Info($"Saldo actual: Q {saldo:N2}");
                    ConsoleUI.Continuar();
                    break;

                case "2":
                    Console.Write("Monto a depositar: ");
                    if (!decimal.TryParse(Console.ReadLine(), out var dep) || dep <= 0)
                        throw new Exception("Monto inválido.");
                    atm.Depositar(numeroCuenta, dep);
                    ConsoleUI.Ok("Depósito exitoso.");
                    ConsoleUI.Continuar();
                    break;

                case "3":
                    Console.Write("Monto a retirar: ");
                    if (!decimal.TryParse(Console.ReadLine(), out var ret) || ret <= 0)
                        throw new Exception("Monto inválido.");
                    atm.Retirar(numeroCuenta, ret);
                    ConsoleUI.Ok("Retiro exitoso.");
                    ConsoleUI.Continuar();
                    break;

                case "4":
                    Console.Write("Cuenta destino: ");
                    var destino = Console.ReadLine()?.Trim() ?? "";
                    Console.Write("Monto a transferir: ");
                    if (!decimal.TryParse(Console.ReadLine(), out var monto) || monto <= 0)
                        throw new Exception("Monto inválido.");
                    atm.Transferir(numeroCuenta, destino, monto);
                    ConsoleUI.Ok("Transferencia realizada.");
                    ConsoleUI.Continuar();
                    break;

                case "5":
                    var h = atm.Historial(numeroCuenta);
                    ConsoleUI.Titulo("Historial de Transacciones");
                    if (h.Count == 0) ConsoleUI.Info("Sin transacciones registradas.");
                    foreach (var t in h.OrderByDescending(x => x.Fecha))
                        Console.WriteLine($"{t.Fecha:g} | {t.Tipo} | Q {t.Monto:N2} | {t.Detalle}");
                    ConsoleUI.Continuar();
                    break;

                case "6":
                    ConsoleUI.Info("Sesión cerrada.");
                    ConsoleUI.Continuar();
                    return;

                default:
                    ConsoleUI.Advertencia("Opción inválida.");
                    ConsoleUI.Continuar();
                    break;
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.Error(ex.Message);
            ConsoleUI.Continuar();
        }
    }
}

void CrearCuenta()
{
    ConsoleUI.Limpiar();
    ConsoleUI.Titulo("Crear Nueva Cuenta");

    Console.Write("Número de cuenta: ");
    var num = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(num))
    {
        ConsoleUI.Advertencia("El número de cuenta es obligatorio.");
        ConsoleUI.Continuar();
        return;
    }
    if (usuarioRepo.ObtenerPorCuenta(num) != null || cuentaRepo.ObtenerPorCuenta(num) != null)
    {
        ConsoleUI.Advertencia("Esa cuenta ya existe.");
        ConsoleUI.Continuar();
        return;
    }

    Console.Write("Nombre del titular: ");
    var nombre = Console.ReadLine()?.Trim() ?? "";

    Console.Write("PIN (4-6 dígitos recomendado): ");
    var pin = ConsoleUI.LeerOculto();
    if (pin.Length < 4)
    {
        ConsoleUI.Advertencia("PIN demasiado corto.");
        ConsoleUI.Continuar();
        return;
    }

    var user = new Usuario
    {
        NumeroCuenta = num,
        Nombre = nombre,
        PinHash = Hashing.Sha256(pin)
    };
    usuarioRepo.Agregar(user);
    cuentaRepo.Agregar(new Cuenta { NumeroCuenta = num, Saldo = 0m });

    ConsoleUI.Ok("Cuenta creada con éxito.");
    ConsoleUI.Continuar();
}