namespace FoodDispenserApp.Services;

public interface IConnectivityService
{
    /// <summary>
    /// Indica si la conexión es local (por ejemplo, se usa WiFi).
    /// </summary>
    bool IsLocal { get; }

    /// <summary>
    /// Método para chequear la conectividad local.
    /// </summary>
    Task<bool> CheckLocalConnectivityAsync();
}
