namespace Utils.OnlineMultiplayer
{
    using Unity.Services.Vivox;
    using UnityEngine;
    using VivoxUnity;
    
    public class VivoxUserHandler : MonoBehaviour
    {
        private IChannelSession _iChannelSession;
        private string _id;
        private string _vivoxId;

        private const int k_volumeMin = -50;
        private const int k_volumeMax = 20;
        
        [SerializeField] private LobbyUserVolumeUI lobbyUserVolumeUI;

        public static float NormalizedVolumeDefault
        {
            get { return (0f - k_volumeMin) / (k_volumeMax - k_volumeMin); }
        }

        public void Start()
        {
            lobbyUserVolumeUI.DisableVoice(true);
        }

        public void SetId(string id)
        {
            _id = id;
            _vivoxId = null;
            
            if(_iChannelSession != null)
            {
                foreach(var participant in _iChannelSession.Participants)
                {
                    if(_id == participant.Account.DisplayName)
                    {
                        _vivoxId = participant.Key;
                        lobbyUserVolumeUI.IsLocalPlayer = participant.IsSelf;
                        lobbyUserVolumeUI.EnableVoice(true);
                        break;
                    }
                }
            }
        }

        public void OnChannelJoined(IChannelSession channelSession)
        {
            _iChannelSession = channelSession;
            _iChannelSession.Participants.AfterKeyAdded += OnParticipantAdded;
            _iChannelSession.Participants.BeforeKeyRemoved += BeforeParticipantRemoved;
            _iChannelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
        }

        public void OnChannelLeft()
        {
            if(_iChannelSession != null)
            {
                _iChannelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
                _iChannelSession.Participants.BeforeKeyRemoved -= BeforeParticipantRemoved;
                _iChannelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
                _iChannelSession = null;
            }
        }
        
        private void OnParticipantAdded(object sender , KeyEventArg<string> keyEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string , IParticipant>)sender;
            var participant = source[keyEventArg.Key];
            var username = participant.Account.DisplayName;

            bool isThisUser = username == _id;
            
            if(isThisUser)
            {
                _vivoxId = keyEventArg.Key;
                lobbyUserVolumeUI.IsLocalPlayer = participant.IsSelf;

                if(!participant.IsMutedForAll)
                    lobbyUserVolumeUI.EnableVoice(false);
                else
                    lobbyUserVolumeUI.DisableVoice(false);
            }
            else
            {
                if(!participant.LocalMute)
                    lobbyUserVolumeUI.EnableVoice(false);
                else
                    lobbyUserVolumeUI.DisableVoice(false);
            }
        }

        private void BeforeParticipantRemoved(object sender , KeyEventArg<string> keyEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string , IParticipant>)sender;
            var participant = source[keyEventArg.Key];
            var username = participant.Account.DisplayName;

            bool isThisUser = username == _id;
            
            if(isThisUser)
            {
                lobbyUserVolumeUI.DisableVoice(true);
            }
        }

        private void OnParticipantValueUpdated(object sender , ValueEventArg<string , IParticipant> valueEventArg)
        {
            var source = (VivoxUnity.IReadOnlyDictionary<string , IParticipant>)sender;
            var participant = source[valueEventArg.Key];
            var username = participant.Account.DisplayName;
            string property = valueEventArg.PropertyName;

            if(username == _id)
            {
                if(property == "UnavailableCaptureDevice")
                {
                    if(participant.UnavailableCaptureDevice)
                    {
                        lobbyUserVolumeUI.DisableVoice(false);
                        participant.SetIsMuteForAll(true , null);
                    }
                    else
                    {
                        lobbyUserVolumeUI.EnableVoice(false);
                        participant.SetIsMuteForAll(false , null);
                    }
                }
                
                else if(property == "IsMutedForAll")
                {
                    if(participant.IsMutedForAll)
                        lobbyUserVolumeUI.DisableVoice(false);
                    else
                        lobbyUserVolumeUI.EnableVoice(false);
                }
            }
        }

        public void OnVolumeSlide(float volumeNormalized)
        {
            if(_iChannelSession == null || _vivoxId == null) return;

            int vol = (int)Mathf.Clamp(k_volumeMin + (k_volumeMax - k_volumeMin) * volumeNormalized , k_volumeMin , k_volumeMax);
            bool isSelf = _iChannelSession.Participants[_vivoxId].IsSelf;
            
            if(isSelf)
            {
                VivoxService.Instance.Client.AudioInputDevices.VolumeAdjustment = vol;
            }
            else
            {
                _iChannelSession.Participants[_vivoxId].LocalVolumeAdjustment = vol;
            }
        }

        public void OnMuteToggle(bool isMuted)
        {
            if(_iChannelSession == null || _vivoxId == null) return;

            bool isSelf = _iChannelSession.Participants[_vivoxId].IsSelf;
            
            if(isSelf)
            {
                VivoxService.Instance.Client.AudioInputDevices.Muted = isMuted;
            }
            else
            {
                _iChannelSession.Participants[_vivoxId].LocalMute = isMuted;
            }
        }
    }
}