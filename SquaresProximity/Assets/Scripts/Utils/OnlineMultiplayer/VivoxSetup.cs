namespace Utils.OnlineMultiplayer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Vivox;
    using VivoxUnity;
    
    public class VivoxSetup
    {
        private bool _hasInitialized;
        private bool _isMidInitialize;
        private IChannelSession _iChannelSession;
        private ILoginSession _iLoginSession;
        private List<VivoxUserHandler> _vivoxUserHandlersList;
        
        public void Initialize(List<VivoxUserHandler> vivoxUserHandlersList , Action<bool> onComplete)
        {
            if(_isMidInitialize) return;
            _isMidInitialize = true;

            _vivoxUserHandlersList = vivoxUserHandlersList;
            VivoxService.Instance.Initialize();
            Account account = new Account(AuthenticationService.Instance.PlayerId);
            _iLoginSession = VivoxService.Instance.Client.GetLoginSession(account);
            string token = _iLoginSession.GetLoginToken();

            _iLoginSession.BeginLogin(token , SubscriptionMode.Accept , null , null , null , result =>
            {
                try
                {
                    _iLoginSession.EndLogin(result);
                    _hasInitialized = true;
                    onComplete?.Invoke(true);
                }
                catch(Exception ex)
                {
                    UnityEngine.Debug.LogWarning("Vivox failed to login: " + ex.Message);
                    onComplete?.Invoke(false);
                }
                finally
                {
                    _isMidInitialize = false;
                }
            });
        }
        
        public void JoinLobbyChannel(string lobbyId , Action<bool> onComplete)
        {
            if(!_hasInitialized || _iLoginSession.State != LoginState.LoggedIn)
            {
                UnityEngine.Debug.LogWarning("Can't join a Vivox audio channel, as Vivox login hasn't completed yet.");
                onComplete?.Invoke(false);
                return;
            }

            ChannelType channelType = ChannelType.NonPositional;
            Channel channel = new Channel(lobbyId + "_voice" , channelType , null);
            _iChannelSession = _iLoginSession.GetChannelSession(channel);
            string token = _iChannelSession.GetConnectToken();

            _iChannelSession.BeginConnect(true , false , true , token , result =>
            {
                try
                {
                    if(_iChannelSession.ChannelState == ConnectionState.Disconnecting || _iChannelSession.ChannelState == ConnectionState.Disconnected)
                    {
                        UnityEngine.Debug.LogWarning("Vivox channel is already disconnecting. Terminating the channel connect sequence.");
                        HandleEarlyDisconnect();
                        return;
                    }

                    _iChannelSession.EndConnect(result);
                    onComplete?.Invoke(true);
                    
                    foreach(VivoxUserHandler userHandler in _vivoxUserHandlersList)
                        userHandler.OnChannelJoined(_iChannelSession);
                }
                catch(Exception ex)
                {
                    UnityEngine.Debug.LogWarning("Vivox failed to connect: " + ex.Message);
                    onComplete?.Invoke(false);
                    _iChannelSession?.Disconnect();
                }
            });
        }
        
        public void LeaveLobbyChannel()
        {
            if(_iChannelSession != null)
            {
                if(_iChannelSession.ChannelState == ConnectionState.Connecting)
                {
                    UnityEngine.Debug.LogWarning("Vivox channel is trying to disconnect while trying to complete its connection. Will wait until connection completes.");
                    HandleEarlyDisconnect();
                    return;
                }

                ChannelId id = _iChannelSession.Channel;
                
                _iChannelSession?.Disconnect((result) =>
                {
                    _iLoginSession.DeleteChannelSession(id);
                    _iChannelSession = null;
                });
            }

            foreach(VivoxUserHandler userHandler in _vivoxUserHandlersList)
                userHandler.OnChannelLeft();
        }

        private void HandleEarlyDisconnect()
        {
            DisconnectOnceConnected();
        }

        async void DisconnectOnceConnected()
        {
            while(_iChannelSession?.ChannelState == ConnectionState.Connecting)
            {
                await Task.Delay(200);
                return;
            }

            LeaveLobbyChannel();
        }
        
        public void Uninitialize()
        {
            if(!_hasInitialized) return;
            
            _iLoginSession.Logout();
        }
    }
}