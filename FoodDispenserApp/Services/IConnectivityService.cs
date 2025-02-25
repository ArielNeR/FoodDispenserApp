namespace FoodDispenserApp.Services;

public interface IConnectivityService
{
    /// <summary>
    /// Método para chequear la conectividad local.
    /// </summary>
    Task<bool> CheckLocalConnectivityAsync();
}