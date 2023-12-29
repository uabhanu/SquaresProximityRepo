namespace Utils.OnlineMultiplayer
{
    using System;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class SymbolKillVolume : MonoBehaviour
    {
        private Action _onSymbolCollided;
        private bool _isInitialized;

        public void Initialize(Action onSymbolCollided)
        {
            _onSymbolCollided = onSymbolCollided;
            _isInitialized = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            if(!_isInitialized) return;

            SymbolObject symbolObj = other.GetComponent<SymbolObject>();

            if(symbolObj != null)
            {
                symbolObj.HideSymbol_ServerRpc();
                _onSymbolCollided?.Invoke();
            }
        }
    }
}
