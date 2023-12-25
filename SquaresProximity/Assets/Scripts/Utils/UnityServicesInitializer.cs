namespace Utils
{
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using UnityEngine;
    
    public class UnityServicesInitializer : MonoBehaviour
    {
        async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized Successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }
            
            if(!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously, User ID: " + AuthenticationService.Instance.PlayerId);
            }
        }
    }
}