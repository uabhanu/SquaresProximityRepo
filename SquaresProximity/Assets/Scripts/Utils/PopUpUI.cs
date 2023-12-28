namespace Utils
{
    using System.Text;
    using TMPro;
    using UnityEngine;
    
    public class PopUpUI : MonoBehaviour
    {
        private float _buttonVisibilityTimeout = -1;
        private StringBuilder _stringBuilder;
        
        [SerializeField] private TMP_InputField popupTMPText;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowPopup(string newText)
        {
            if(!gameObject.activeSelf)
            {   _stringBuilder.Clear();
                gameObject.SetActive(true);
            }
            
            _stringBuilder.AppendLine(newText);
            popupTMPText.SetTextWithoutNotify(_stringBuilder.ToString());
            DisableButton();
        }

        private void DisableButton()
        {
            _buttonVisibilityTimeout = 0.5f;
            canvasGroup.alpha = 0.5f;
            canvasGroup.interactable = false;
        }
        private void ReenableButton()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }

        private void Update()
        {
            if(_buttonVisibilityTimeout >= 0 && _buttonVisibilityTimeout - Time.deltaTime < 0) ReenableButton();
            
            _buttonVisibilityTimeout -= Time.deltaTime;
        }

        public void ClearPopup()
        {
            gameObject.SetActive(false);
        }
    }
}