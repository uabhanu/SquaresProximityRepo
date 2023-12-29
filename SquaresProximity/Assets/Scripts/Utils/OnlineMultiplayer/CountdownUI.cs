namespace Utils.OnlineMultiplayer
{
    using Managers;
    using TMPro;
    using UnityEngine;
    
    public class CountdownUI : OnlineMultiplayerUIManager
    {
        [SerializeField] TMP_Text countDownTMPText;

        public void OnTimeChanged(float time)
        {
            if(time <= 0)
                countDownTMPText.SetText("Waiting for all players...");
            else
                countDownTMPText.SetText($"Starting in: {time:0}");
        }
    }
}