using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class UnityServicesManager : MonoBehaviour
{
    async void Awake()
    {
        await InitializeServices();
    }

    async Task InitializeServices()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Unity Services Initialized & Signed In");
    }
}