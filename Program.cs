using CajeroApp.models;
using CajeroApp.Repos;
using CajeroApp.Services;
using CajeroApp.Utils;

// =========== CONFIGURAR RUTAS A /data ===========
var projectRoot = Directory.GetParent(AppContext.BaseDirectory)
                    ?.Parent
                    ?.Parent
                    ?.Parent
                    ?.FullName
                ?? AppContext.BaseDirectory;

var dataDir = Path.Combine(projectRoot, "data");
Directory.CreateDirectory(dataDir);

var usuariosPath = Path.Combine(dataDir, "usuarios.json");
var cuentasPath = Path.Combine(dataDir, "cuentas.json");
var transPath = Path.Combine(dataDir, "transacciones.json");

// repos / servicios
var usuarioRepo = new UsuarioRepo(usuariosPath);
var cuentaRepo = new CuentaRepo(cuentasPath);
var transRepo = new TransaccionRepo(transPath);

var authService = new AuthService(usuarioRepo);
var atmService = new AtmService(cuentaRepo, transRepo, usuarioRepo);

// datos iniciales
Semilla(usuarioRepo, cuentaRepo);

// =========== LOOP MENÚ PRINCIPAL ===========
while (true)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("+----------------------------------+");
    Console.WriteLine("| CAJERO AUTOMÁTICO -MENÚ PRINCIPAL|");
    Console.WriteLine("+----------------------------------+");

    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    
    Console.WriteLine($"   |Fecha:{DateTime.Now:dd/MM/yyyy} Hora:{DateTime.Now:HH:mm}|");
    
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(" [1] Iniciar sesión              ");      
    Console.WriteLine(" [2] Crear nueva cuenta          ");
    Console.WriteLine(" [3] Salir                       ");
    
    Console.WriteLine();
    Console.Write("Seleccione una opción: ");
    string? opcion = Console.ReadLine();

    if (opcion == "1")
    {
        var sesion = IniciarSesion(authService, cuentaRepo);
        if (sesion.usuario != null && sesion.cuenta != null)
        {
            if (sesion.usuario.IsAdmin)
                MenuAdmin(sesion.usuario, sesion.cuenta);
            else
                MenuUsuario(sesion.usuario, sesion.cuenta);
        }
    }
    else if (opcion == "2")
    {
        CrearCuentaFlujo(atmService);
    }
    else if (opcion == "3")
    {
        Console.WriteLine();
        Console.WriteLine("[OK] Gracias por usar el sistema.");
        Pausa();
        break;
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("[ERROR] Opción inválida.");
        Pausa();
    }
}

// =========== FUNCIONES ===========

(Usuario? usuario, Cuenta? cuenta) IniciarSesion(AuthService auth, CuentaRepo cuentaRepo)
{
    Console.Clear();
    Console.WriteLine("====================================");
    Console.WriteLine(" INICIAR SESIÓN ");
    Console.WriteLine("====================================");

    Console.Write("Número de cuenta: ");
    string numeroCuenta = Console.ReadLine() ?? "";

    Console.Write("PIN: ");
    string pin = LeerPinOculto();

    var usuario = auth.Autenticar(numeroCuenta, pin);
    if (usuario == null)
    {
        Console.WriteLine();
        Console.WriteLine("[ERROR] Credenciales inválidas o cuenta bloqueada.");
        Pausa();
        return (null, null);
    }

    var cuenta = cuentaRepo.ObtenerPorNumeroCuenta(usuario.NumeroCuenta);
    if (cuenta == null)
    {
        Console.WriteLine();
        Console.WriteLine("[ERROR] No se encontró la cuenta asociada.");
        Pausa();
        return (null, null);
    }

    Console.WriteLine();
    Console.WriteLine("[OK] Inicio de sesión correcto.");
    Pausa();
    return (usuario, cuenta);
}

void MenuUsuario(Usuario usuario, Cuenta cuenta)
{
    while (true)
    {
        cuenta = cuentaRepo.ObtenerPorNumeroCuenta(cuenta.NumeroCuenta)!;

        Console.Clear();
        Console.WriteLine("====================================");
        Console.WriteLine(" SESIÓN DE USUARIO ");
        Console.WriteLine("====================================");
        Console.WriteLine($"Cuenta : {cuenta.NumeroCuenta}");
        Console.WriteLine($"Titular: {usuario.Nombre}");
        Console.WriteLine($"Saldo  : Q {cuenta.Saldo:F2}");
        Console.WriteLine("------------------------------------");
        Console.WriteLine("[1] Consultar saldo");
        Console.WriteLine("[2] Depositar");
        Console.WriteLine("[3] Retirar");
        Console.WriteLine("[4] Transferir");
        Console.WriteLine("[5] Ver historial");
        Console.WriteLine("[6] Cerrar sesión");
        Console.WriteLine();
        Console.Write("Seleccione una opción: ");
        string? opcion = Console.ReadLine();

        if (opcion == "1")
        {
            ConsultarSaldo(cuenta);
        }
        else if (opcion == "2")
        {
            DepositarPropio(atmService, cuenta);
        }
        else if (opcion == "3")
        {
            Retirar(atmService, cuenta);
        }
        else if (opcion == "4")
        {
            Transferir(atmService, cuenta);
        }
        else if (opcion == "5")
        {
            VerHistorialPropio(atmService, cuenta);
        }
        else if (opcion == "6")
        {
            Console.WriteLine();
            Console.WriteLine("[AVISO] Sesión cerrada.");
            Pausa();
            break;
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("[ERROR] Opción inválida.");
            Pausa();
        }
    }
}

void MenuAdmin(Usuario admin, Cuenta cuentaAdmin)
{
    while (true)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("{====================================");
        Console.WriteLine(" PANEL DE ADMINISTRACIÓN ");
        Console.WriteLine("====================================");
        Console.WriteLine($"Admin  : {admin.Nombre}");
        Console.WriteLine($"Cuenta : {admin.NumeroCuenta}");
        Console.WriteLine("------------------------------------");
        Console.WriteLine("[1] Ver todas las cuentas");
        Console.WriteLine("[2] Ver historial de una cuenta");
        Console.WriteLine("[3] Depositar en una cuenta (ajuste)");
        Console.WriteLine("[4] Crear nueva cuenta");
        Console.WriteLine("[5] Desbloquear cuenta");
        Console.WriteLine("[6] Cerrar sesión admin");
        Console.WriteLine();
        Console.Write("Seleccione una opción: ");
        string? opcion = Console.ReadLine();

        if (opcion == "1")
        {
            AdminListarCuentas();
        }
        else if (opcion == "2")
        {
            AdminVerHistorialCuenta();
        }
        else if (opcion == "3")
        {
            AdminDepositarEnCuenta(admin);
        }
        else if (opcion == "4")
        {
            CrearCuentaFlujo(atmService, esAdminCreando: true);
        }
        else if (opcion == "5")
        {
            AdminDesbloquearCuenta();
        }
        else if (opcion == "6")
        {
            Console.WriteLine();
            Console.WriteLine("[AVISO] Sesión admin cerrada.");
            Pausa();
            break;
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("[ERROR] Opción inválida.");
            Pausa();
        }
    }
}

void ConsultarSaldo(Cuenta cuenta)
{
    Console.Clear();
    Console.WriteLine("---------- CONSULTA DE SALDO ----------");
    Console.WriteLine($"Cuenta: {cuenta.NumeroCuenta}");
    Console.WriteLine($"Saldo : Q {cuenta.Saldo:F2}");
    Pausa();
}

void DepositarPropio(AtmService atm, Cuenta cuenta)
{
    Console.Clear();
    Console.WriteLine("---------- DEPÓSITO ----------");
    Console.Write("Monto a depositar (Q): ");
    if (!decimal.TryParse(Console.ReadLine(), out var monto) || monto <= 0)
    {
        Console.WriteLine("[ERROR] Monto inválido.");
        Pausa();
        return;
    }

    bool ok = atm.Depositar(cuenta.NumeroCuenta, monto);
    if (!ok)
    {
        Console.WriteLine("[ERROR] No se pudo completar el depósito.");
        Pausa();
        return;
    }

    var actualizada = cuentaRepo.ObtenerPorNumeroCuenta(cuenta.NumeroCuenta)!;
    Console.WriteLine("[OK] Depósito realizado.");
    Console.WriteLine($"Nuevo saldo: Q {actualizada.Saldo:F2}");
    Pausa();
}

void Retirar(AtmService atm, Cuenta cuenta)
{
    Console.Clear();
    Console.WriteLine("---------- RETIRO ----------");
    Console.WriteLine($"Saldo disponible: Q {cuenta.Saldo:F2}");
    Console.Write("Monto a retirar (Q): ");
    if (!decimal.TryParse(Console.ReadLine(), out var monto) || monto <= 0)
    {
        Console.WriteLine("[ERROR] Monto inválido.");
        Pausa();
        return;
    }

    bool ok = atm.Retirar(cuenta.NumeroCuenta, monto);
    if (!ok)
    {
        Console.WriteLine("[ERROR] No se pudo retirar (saldo insuficiente o límite excedido).");
        Pausa();
        return;
    }

    var actualizada = cuentaRepo.ObtenerPorNumeroCuenta(cuenta.NumeroCuenta)!;
    Console.WriteLine("[OK] Retiro realizado.");
    Console.WriteLine($"Nuevo saldo: Q {actualizada.Saldo:F2}");
    Pausa();
}

void Transferir(AtmService atm, Cuenta origen)
{
    Console.Clear();
    Console.WriteLine("---------- TRANSFERENCIA ----------");
    Console.Write("Cuenta destino: ");
    string destino = Console.ReadLine() ?? "";

    Console.Write("Monto a transferir (Q): ");
    if (!decimal.TryParse(Console.ReadLine(), out var monto) || monto <= 0)
    {
        Console.WriteLine("[ERROR] Monto inválido.");
        Pausa();
        return;
    }

    bool ok = atm.Transferir(origen.NumeroCuenta, destino, monto);
    if (!ok)
    {
        Console.WriteLine("[ERROR] No se pudo transferir (saldo insuficiente o cuenta destino no existe).");
        Pausa();
        return;
    }

    var actualizada = cuentaRepo.ObtenerPorNumeroCuenta(origen.NumeroCuenta)!;
    Console.WriteLine("[OK] Transferencia realizada.");
    Console.WriteLine($"Nuevo saldo: Q {actualizada.Saldo:F2}");
    Pausa();
}

void VerHistorialPropio(AtmService atm, Cuenta cuenta)
{
    Console.Clear();
    Console.WriteLine("---------- HISTORIAL ----------");

    var lista = atm.ObtenerHistorial(cuenta.NumeroCuenta);

    if (lista.Count == 0)
    {
        Console.WriteLine("No hay transacciones todavía.");
        Pausa();
        return;
    }

    foreach (var t in lista)
    {
        Console.WriteLine("------------------------------------");
        Console.WriteLine($"Fecha : {t.Fecha:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"Tipo  : {t.Tipo}");
        Console.WriteLine($"Monto : Q {t.Monto:F2}");
        if (!string.IsNullOrWhiteSpace(t.CuentaDestino))
        {
            Console.WriteLine($"Destino: {t.CuentaDestino}");
        }
    }
    Console.WriteLine("------------------------------------");
    Pausa();
}

// =========== FUNCIONES ADMIN ===========

void AdminListarCuentas()
{
    Console.Clear();
    Console.WriteLine("---------- TODAS LAS CUENTAS ----------");

    var cuentas = atmService.ListarTodasLasCuentas();
    if (cuentas.Count == 0)
    {
        Console.WriteLine("No hay cuentas registradas.");
    }
    else
    {
        foreach (var c in cuentas)
        {
            Console.WriteLine($"Cuenta: {c.NumeroCuenta} | Saldo: Q {c.Saldo:F2}");
        }
    }
    Pausa();
}

void AdminVerHistorialCuenta()
{
    Console.Clear();
    Console.WriteLine("---------- HISTORIAL DE CUENTA ----------");
    Console.Write("Número de cuenta: ");
    string num = Console.ReadLine() ?? "";

    var lista = atmService.ObtenerHistorial(num);

    if (lista.Count == 0)
    {
        Console.WriteLine("No hay transacciones o la cuenta no existe.");
        Pausa();
        return;
    }

    foreach (var t in lista)
    {
        Console.WriteLine("------------------------------------");
        Console.WriteLine($"Cuenta: {t.NumeroCuenta}");
        Console.WriteLine($"Fecha : {t.Fecha:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"Tipo  : {t.Tipo}");
        Console.WriteLine($"Monto : Q {t.Monto:F2}");
        if (!string.IsNullOrWhiteSpace(t.CuentaDestino))
        {
            Console.WriteLine($"Destino: {t.CuentaDestino}");
        }
    }
    Console.WriteLine("------------------------------------");
    Pausa();
}

void AdminDepositarEnCuenta(Usuario admin)
{
    Console.Clear();
    Console.WriteLine("---------- AJUSTE ADMIN ----------");

    Console.Write("Cuenta a acreditar: ");
    string num = Console.ReadLine() ?? "";

    Console.Write("Monto a acreditar (Q): ");
    if (!decimal.TryParse(Console.ReadLine(), out var monto) || monto <= 0)
    {
        Console.WriteLine("[ERROR] Monto inválido.");
        Pausa();
        return;
    }

    bool ok = atmService.Depositar(num, monto, adminQueHaceEsto: admin.NumeroCuenta);

    if (!ok)
    {
        Console.WriteLine("[ERROR] No se pudo acreditar (cuenta no existe).");
    }
    else
    {
        Console.WriteLine("[OK] Acreditado correctamente.");
    }

    Pausa();
}

void AdminDesbloquearCuenta()
{
    Console.Clear();
    Console.WriteLine("---------- DESBLOQUEAR CUENTA ----------");
    Console.Write("Número de cuenta a desbloquear: ");
    string num = Console.ReadLine() ?? "";

    bool ok = atmService.DesbloquearCuenta(num);

    if (!ok)
    {
        Console.WriteLine("[ERROR] No se pudo desbloquear (no existe o ya estaba activa).");
    }
    else
    {
        Console.WriteLine("[OK] Cuenta desbloqueada.");
    }

    Pausa();
}

// =========== CREAR CUENTA (para menú principal y admin) ===========
void CrearCuentaFlujo(AtmService atm, bool esAdminCreando = false)
{
    Console.Clear();
    Console.WriteLine("---------- CREAR CUENTA ----------");

    Console.Write("Número de cuenta nuevo: ");
    string num = Console.ReadLine() ?? "";

    Console.Write("Nombre del titular: ");
    string nombre = Console.ReadLine() ?? "";

    Console.Write("PIN nuevo: ");
    string pin = LeerPinOculto();

    bool creada = atm.CrearCuenta(num, nombre, pin, isAdmin: esAdminCreando == true ? false : false);
    // Nota: arriba dejé que incluso el admin cree cuentas normales, no admins. Si quieres que el admin pueda crear otro admin,
    // cambia a: isAdmin: esAdminCreando

    if (!creada)
    {
        Console.WriteLine("[ERROR] No se pudo crear (ya existe o datos inválidos).");
    }
    else
    {
        Console.WriteLine("[OK] Cuenta creada correctamente.");
    }

    Pausa();
}

// =========== UTILS LOCALES Program.cs ===========

void Pausa()
{
    Console.WriteLine();
    Console.Write("Presione una tecla para continuar...");
    Console.ReadKey(true);
}

string LeerPinOculto()
{
    var buffer = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0)
            {
                buffer.Length--;
                Console.Write("\b \b");
            }
        }
        else if (char.IsDigit(key.KeyChar))
        {
            buffer.Append(key.KeyChar);
            Console.Write("*");
        }
    }
    return buffer.ToString();
}

void Semilla(UsuarioRepo usuarioRepo, CuentaRepo cuentaRepo)
{
    // si ya existen datos, no tocamos nada
    if (usuarioRepo.ObtenerTodos().Count > 0 || cuentaRepo.ObtenerTodas().Count > 0)
        return;

    // crear usuario normal demo
    var usuarioNormal = new Usuario
    {
        NumeroCuenta = "1001",
        Nombre = "Usuario Demo",
        PinHash = Hashing.Sha256("1234"),
        IsAdmin = false,
        IntentosFallidos = 0,
        Bloqueada = false
    };

    var cuentaNormal = new Cuenta
    {
        NumeroCuenta = "1001",
        Saldo = 1500.00m
    };

    // crear admin demo
    var usuarioAdmin = new Usuario
    {
        NumeroCuenta = "9999",
        Nombre = "Administrador",
        PinHash = Hashing.Sha256("0000"),
        IsAdmin = true,
        IntentosFallidos = 0,
        Bloqueada = false
    };

    var cuentaAdmin = new Cuenta
    {
        NumeroCuenta = "9999",
        Saldo = 0m
    };

    usuarioRepo.Agregar(usuarioNormal);
    cuentaRepo.Agregar(cuentaNormal);

    usuarioRepo.Agregar(usuarioAdmin);
    cuentaRepo.Agregar(cuentaAdmin);
}