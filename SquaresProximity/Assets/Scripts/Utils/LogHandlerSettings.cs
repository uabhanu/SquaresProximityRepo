namespace Utils
{
    using UnityEngine;
    
    public class LogHandlerSettings : MonoBehaviour
    {
        private static LogHandlerSettings _logHandlerSettings;
        
        [SerializeField] [Tooltip("Only logs of this level or higher will appear in the console.")] private LogMode editorLogVerbosity = LogMode.Critical;
        [SerializeField] private PopUpUI popUpUI;
        
        public static LogHandlerSettings Instance
        {
            get
            {
                if(_logHandlerSettings != null) return _logHandlerSettings;
                
                return _logHandlerSettings = FindObjectOfType<LogHandlerSettings>();
            }
        }
        
        private void Awake()
        {
            LogHandler.Get().LogMode = editorLogVerbosity;
            Debug.Log($"Starting project with Log Level : {editorLogVerbosity.ToString()}");
        }
        
        public void OnValidate()
        {
            LogHandler.Get().LogMode = editorLogVerbosity;
        }
        
        public void SpawnErrorPopup(string errorMessage)
        {
            popUpUI.ShowPopup(errorMessage);
        }
    }
}