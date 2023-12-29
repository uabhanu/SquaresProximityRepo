namespace Utils.OnlineMultiplayer
{
    using Unity.Netcode;
    using Unity.Netcode.Components;
    using UnityEngine;
    
    [RequireComponent(typeof(NetworkTransform))]
    public class SymbolContainer : NetworkBehaviour
    {
        private bool _isConnected;
        private bool _hasGameStarted;
        
        [SerializeField] float speed = 1;
        
        private void Update()
        {
            if(!IsHost) return;
            
            if(!_hasGameStarted) return;
            
            BeginMotion();
        }

        private void BeginMotion()
        {
            transform.position += Time.deltaTime * speed * Vector3.down;
        }
        
        public override void OnNetworkSpawn()
        {
            if(IsHost)
            {
                _isConnected = true;
                transform.position = Vector3.up * 10;
            }
            else
            {
                enabled = false;
            }
        }
        
        public void StartMovingSymbols()
        {
            _hasGameStarted = true;
            
            if(_isConnected) BeginMotion();
        }
    }
}