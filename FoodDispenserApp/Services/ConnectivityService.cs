using Microsoft.Maui.Networking;

namespace FoodDispenserApp.Services;

public class ConnectivityService : IConnectivityService
{
    // Para simplificar, se asume que si el dispositivo está conectado vía WiFi se considera en modo local.
    public bool IsLocal => CheckLocalConnectivityAsync().Result;

    public async Task<bool> CheckLocalConnectivityAsync()
    {
        var access = Connectivity.Current.NetworkAccess;
        if (access == NetworkAccess.Internet)
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            if (profiles.Contains(ConnectionProfile.WiFi))
            {
                return true;
            }
        }
        return false;
    }
}
