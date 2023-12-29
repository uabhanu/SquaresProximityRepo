namespace Utils.OnlineMultiplayer
{
    using UnityEngine;
    using UnityEngine.UI;
    
    public class LobbyUserVolumeUI : MonoBehaviour
    {
        [SerializeField] MultiplayerUIPanel volumeSliderContainer;
        [SerializeField] MultiplayerUIPanel muteToggleContainer;
        [SerializeField] [Tooltip("This is shown for other players, to mute them.")] GameObject muteIcon;
        [SerializeField] [Tooltip("This is shown for the local player, to make it clearer that they are muting themselves.")] GameObject micMuteIcon;
        [SerializeField] Slider volumeSlider;
        [SerializeField] Toggle muteToggle;
        
        public bool IsLocalPlayer { private get; set; }
        
        public void EnableVoice(bool shouldResetUi)
        {
            if(shouldResetUi)
            {   volumeSlider.SetValueWithoutNotify(VivoxUserHandler.NormalizedVolumeDefault);
                muteToggle.SetIsOnWithoutNotify(false);
            }

            if(IsLocalPlayer)
            {
                volumeSliderContainer.Hide(0);
                muteToggleContainer.Show();
                muteIcon.SetActive(false);
                micMuteIcon.SetActive(true);
            }
            else
            {
                volumeSliderContainer.Show();
                muteToggleContainer.Show();
                muteIcon.SetActive(true);
                micMuteIcon.SetActive(false);
            }
        }
        
        public void DisableVoice(bool shouldResetUi)
        {
            if(shouldResetUi)
            {   volumeSlider.value = VivoxUserHandler.NormalizedVolumeDefault;
                muteToggle.isOn = false;
            }

            volumeSliderContainer.Hide(0.4f);
            muteToggleContainer.Hide(0.4f);
            muteIcon.SetActive(true);
            micMuteIcon.SetActive(false);
        }
    }
}