namespace Utils
{
    using Managers;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    
    [RequireComponent(typeof(CanvasGroup))]
    public class MultiplayerUIPanel : MonoBehaviour
    {
        private bool _showing;
        private CanvasGroup _canvasGroup;
        private OnlineMultiplayerUIManager _onlineMultiplayerUIManager;
        private List<MultiplayerUIPanel> _uiPanelsInChildren;
        
        [SerializeField] private UnityEvent<bool> onVisibilityChange;

        protected OnlineMultiplayerUIManager onlineMultiplayerUIManager
        {
            get
            {
                if (_onlineMultiplayerUIManager != null) return _onlineMultiplayerUIManager;
                return _onlineMultiplayerUIManager = OnlineMultiplayerUIManager.Instance;
            }
        }

        public void Start()
        {
            var children = GetComponentsInChildren<MultiplayerUIPanel>(true);
            
            foreach(var child in children)
                if(child != this)
                    _uiPanelsInChildren.Add(child);
        }

        private CanvasGroup MyCanvasGroup
        {
            get
            {
                if (_canvasGroup != null) return _canvasGroup;
                return _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public void Toggle()
        {
            if (_showing)
                Hide();
            else
                Show();
        }


        public void Show()
        {
            Show(true);
        }

        public void Show(bool propagateToChildren)
        {
            MyCanvasGroup.alpha = 1;
            MyCanvasGroup.interactable = true;
            MyCanvasGroup.blocksRaycasts = true;
            _showing = true;
            onVisibilityChange?.Invoke(true);
            
            if(!propagateToChildren)
                return;
            
            foreach(MultiplayerUIPanel child in _uiPanelsInChildren)
                child.onVisibilityChange?.Invoke(true);
        }

        public void Hide()
        {
            Hide(0);
        }

        public void Hide(float targetAlpha)
        {
            MyCanvasGroup.alpha = targetAlpha;
            MyCanvasGroup.interactable = false;
            MyCanvasGroup.blocksRaycasts = false;
            _showing = false;
            onVisibilityChange?.Invoke(false);
            
            foreach(MultiplayerUIPanel child in _uiPanelsInChildren)
                child.onVisibilityChange?.Invoke(false);
        }
    }
}
