using Microsoft.Maui.Networking;

namespace FoodDispenserApp.Services;

public class ConnectivityService : IConnectivityService
{
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