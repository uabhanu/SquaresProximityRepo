namespace Utils
{
    using UnityEngine;
    using UnityEngine.UI;

    public class RadioButton : MonoBehaviour
    {
        public Toggle toggle;

        private void Start()
        {
            if(toggle == null)
            {
                Debug.LogError("Toggle component not assigned to RadioButton script.");
                return;
            }

            toggle.group = GetComponentInParent<ToggleGroup>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if(isOn)
            {
                Debug.Log("Toggle " + toggle.name + " is now " + (isOn ? "On" : "Off"));
            }
        }
    }
}