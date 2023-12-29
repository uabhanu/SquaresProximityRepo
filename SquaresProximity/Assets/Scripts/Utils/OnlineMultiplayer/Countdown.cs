namespace Utils.OnlineMultiplayer
{
    using Managers;
    using UnityEngine;
    
    [RequireComponent(typeof(CountdownUI))]
    public class Countdown : MonoBehaviour
    {
        private CallbackValue<float> _timeLeft;
        private CountdownUI _countdownUI;
        private const int _countdownTime = 4;

        public void OnEnable()
        {
            if(_countdownUI == null) _countdownUI = GetComponent<CountdownUI>();
            
            _timeLeft.OnChanged += _countdownUI.OnTimeChanged;
            _timeLeft.Value = -1;
        }

        public void StartCountDown()
        {
            _timeLeft.Value = _countdownTime;
        }

        public void CancelCountDown()
        {
            _timeLeft.Value = -1;
        }

        public void Update()
        {
            if(_timeLeft.Value < 0) return;
            
            _timeLeft.Value -= Time.deltaTime;
            
            if(_timeLeft.Value < 0) OnlineMultiplayerUIManager.Instance.FinishedCountDown();
        }
    }
}