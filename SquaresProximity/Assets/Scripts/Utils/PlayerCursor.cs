namespace Utils
{
    using System;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;
    /// <summary>
    /// This cursor object is nothing but the Mouse Trail Object
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerCursor : NetworkBehaviour
    {
        private Action<ulong , Action<PlayerData>> _retrieveNameAction;
        private Camera _mainCamera;
        private List<SymbolObject> _currentlyCollidingSymbolObjectsList;
        private NetworkVariable<Vector3> _position;
        private ulong _localId;
        
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] ParticleSystem onClickParticles;
        [SerializeField] TMPro.TMP_Text nameOutputTMP;
        
        public override void OnNetworkSpawn()
        {
            _retrieveNameAction = NetworkedDataStore.Instance.GetPlayerData;
            _mainCamera = GameObject.Find("MaineCamera").GetComponent<Camera>();
            InGameRunner.Instance.OnGameBeginning += OnGameBegan;
            
            if(IsHost) _currentlyCollidingSymbolObjectsList = new List<SymbolObject>();
            _localId = NetworkManager.Singleton.LocalClientId;
            
            if(OwnerClientId != _localId)
            {
                spriteRenderer.transform.localScale *= 0.75f;
                spriteRenderer.color = new Color(1 , 1 , 1 , 0.5f);
                var trails = onClickParticles.trails;
                trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(Color.grey);
            }
            else
            {
                spriteRenderer.enabled = false;
            }
        }

        [ClientRpc]
        private void SetName_ClientRpc(PlayerData data)
        {
            if(!IsOwner) nameOutputTMP.text = data.Name;
        }
        
        private bool IsSelectInputHit()
        {
            return Input.GetMouseButtonDown(0);
        }

        public void Update()
        {
            transform.position = _position.Value;
            
            if(_mainCamera == null || !IsOwner) return;

            Vector3 targetPos = (Vector2)_mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x , Input.mousePosition.y , -_mainCamera.transform.position.z));
            SetPosition_ServerRpc(targetPos);
            
            if(IsSelectInputHit()) SendInput_ServerRpc(_localId);
        }

        [ServerRpc]
        private void SetPosition_ServerRpc(Vector3 position)
        {
            _position.Value = position;
        }

        [ServerRpc]
        private void SendInput_ServerRpc(ulong id)
        {
            if(_currentlyCollidingSymbolObjectsList.Count > 0)
            {
                SymbolObject symbol = _currentlyCollidingSymbolObjectsList[0];
                InGameRunner.Instance.OnPlayerInput(id , symbol);
            }

            OnInputVisuals_ClientRpc();
        }

        [ClientRpc]
        private void OnInputVisuals_ClientRpc()
        {
            onClickParticles.Stop(false , ParticleSystemStopBehavior.StopEmitting);
            onClickParticles.Play();
        }

        public void OnTriggerEnter(Collider other)
        {
            if(!IsHost) return;
            
            SymbolObject symbol = other.GetComponent<SymbolObject>();
            
            if(symbol == null) return;
            
            if(!_currentlyCollidingSymbolObjectsList.Contains(symbol)) _currentlyCollidingSymbolObjectsList.Add(symbol);
        }

        public void OnTriggerExit(Collider other)
        {
            if(!IsHost) return;
            
            SymbolObject symbol = other.GetComponent<SymbolObject>();
            
            if(symbol == null) return;
            
            if(_currentlyCollidingSymbolObjectsList.Contains(symbol)) _currentlyCollidingSymbolObjectsList.Remove(symbol);
        }

        public void OnGameBegan()
        {
            _retrieveNameAction.Invoke(OwnerClientId, SetName_ClientRpc);
            InGameRunner.Instance.OnGameBeginning -= OnGameBegan;
        }
    }
}