namespace Utils
{
    using Managers;
    using UnityEngine;
    using UnityEngine.UI;

    public class RadioButton : MonoBehaviour
    {
        public Toggle toggle;
        
        private void Start()
        {
            if(toggle == null)
            {
                return;
            }
        
            toggle.group = GetComponentInParent<ToggleGroup>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            if(isOn)
            {
                EventsManager.Invoke(Managers.Event.NumberOfPlayersToggled);   
            }
        }
    }
}