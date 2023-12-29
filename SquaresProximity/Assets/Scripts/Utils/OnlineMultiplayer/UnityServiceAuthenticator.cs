namespace Utils.OnlineMultiplayer
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    
    public static class UnityServiceAuthenticator
    {
        private const int _initTimeout = 10000;
        private static bool _isSigningIn;
        
        public static async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            if(UnityServices.State == ServicesInitializationState.Initialized) return true;
            
            if(UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                
                if(await Task.WhenAny(task, Task.Delay(_initTimeout)) != task) return false;

                return UnityServices.State == ServicesInitializationState.Initialized;
            }

            if(profileName != null)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName , "");
                var authProfile = new InitializationOptions().SetProfile(profileName);
                await UnityServices.InitializeAsync(authProfile);
            }
            else
                await UnityServices.InitializeAsync();

            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized) await Task.Delay(100);
            }
        }

        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            if(!await TryInitServicesAsync(profileName)) return false;
            
            if(_isSigningIn)
            {
                var task = WaitForSignedIn();
                
                if(await Task.WhenAny(task, Task.Delay(_initTimeout)) != task) return false;
                return AuthenticationService.Instance.IsSignedIn;
            }

            _isSigningIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _isSigningIn = false;

            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while(!AuthenticationService.Instance.IsSignedIn) await Task.Delay(100);
            }
        }
    }
}