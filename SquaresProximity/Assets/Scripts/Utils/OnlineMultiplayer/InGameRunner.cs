namespace Utils.OnlineMultiplayer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Managers;
    using Unity.Netcode;
    using UnityEngine;
    
    public class InGameRunner : NetworkBehaviour
    {
        private Action _onConnectionVerified;
        private Action _onGameEnd;
        private bool _hasConnected;
        private bool? _canSpawnInGameObjects;
        private float _symbolSpawnTimer = 0.5f;
        private int _expectedPlayerCount;
        private int _remainingSymbolCount;
        private float _timeout = 10;
        private PlayerData _playerData;
        private Queue<Vector2> _pendingSymbolPositionsQueue;
        private static InGameRunner _instance;
        
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private PlayerCursor playerCursorPrefab;
        [SerializeField] private SymbolContainer symbolContainerInstance;
        [SerializeField] private SymbolContainer symbolContainerPrefab;
        [SerializeField] private SymbolObject symbolObjectPrefab;
        [SerializeField] private SequenceSelector sequenceSelector;
        [SerializeField] private Scorer scorer;
        [SerializeField] private SymbolKillVolume symbolKillVolume;
        [SerializeField] private IntroOutroRunner introOutroRunner;
        [SerializeField] private NetworkedDataStore networkedDataStore;

        public Action OnGameBeginning;

        public static InGameRunner Instance
        {
            get
            {
                if(_instance!) return _instance;
                return _instance = FindObjectOfType<InGameRunner>();
            }
        }

        public void Initialize(Action onConnectionVerified , int expectedPlayerCount , Action onGameBegin , Action onGameEnd , LocalPlayer localUser)
        {
            _onConnectionVerified = onConnectionVerified;
            _expectedPlayerCount = expectedPlayerCount;
            OnGameBeginning = onGameBegin;
            _onGameEnd = onGameEnd;
            _canSpawnInGameObjects = null;
            _playerData = new PlayerData(localUser.DisplayName.Value , 0);
        }

        public override void OnNetworkSpawn()
        {
            if(IsHost) FinishInitialize();
            _playerData = new PlayerData(_playerData.Name , NetworkManager.Singleton.LocalClientId);
            VerifyConnection_ServerRpc(_playerData.Id);
        }

        public override void OnNetworkDespawn()
        {
            _onGameEnd();
        }

        private void FinishInitialize()
        {
            symbolContainerInstance = Instantiate(symbolContainerPrefab);
            symbolContainerInstance.NetworkObject.Spawn();
            ResetPendingSymbolPositions();
            symbolKillVolume.Initialize(OnSymbolDeactivated);
        }

        private void ResetPendingSymbolPositions()
        {
            _pendingSymbolPositionsQueue.Clear();
            Rect boxRext = new Rect(boxCollider.bounds.min.x , boxCollider.bounds.min.y , boxCollider.bounds.size.x , boxCollider.bounds.size.y);
            IList<Vector2> points = sequenceSelector.GenerateRandomSpawnPoints(boxRext , 2);
            
            foreach(Vector2 point in points)
                _pendingSymbolPositionsQueue.Enqueue(point);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);
        }

        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if(clientId == _playerData.Id)
                VerifyConnectionConfirm_ServerRpc(_playerData);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(PlayerData clientData)
        {
            PlayerCursor playerCursor = Instantiate(playerCursorPrefab);
            playerCursor.NetworkObject.SpawnWithOwnership(clientData.Id);
            playerCursor.name += clientData.Name;
            networkedDataStore.AddPlayer(clientData.Id , clientData.Name);
            bool areAllPlayersConnected = NetworkManager.Singleton.ConnectedClients.Count >= _expectedPlayerCount;
            VerifyConnectionConfirm_ClientRpc(clientData.Id , areAllPlayersConnected);
        }

        [ClientRpc]
        private void VerifyConnectionConfirm_ClientRpc(ulong clientId , bool canBeginGame)
        {
            if(clientId == _playerData.Id)
            {
                _onConnectionVerified?.Invoke();
                _hasConnected = true;
            }

            if(canBeginGame && _hasConnected)
            {
                _timeout = -1;
                BeginGame();
            }
        }

        private void BeginGame()
        {
            _canSpawnInGameObjects = true;
            OnlineMultiplayerUIManager.Instance.BeginGame();
            OnGameBeginning?.Invoke();
            introOutroRunner.DoIntro(StartMovingSymbols);
        }

        private void StartMovingSymbols()
        {
            sequenceSelector.SetTargetsAnimatable();
            
            if(IsHost)
                symbolContainerInstance.StartMovingSymbols();
        }

        public void Update()
        {
            CheckIfCanSpawnNewSymbol();
            
            if(_timeout >= 0)
            {
                _timeout -= Time.deltaTime;
                
                if(_timeout < 0)
                    BeginGame();
            }

            return;

            void CheckIfCanSpawnNewSymbol()
            {
                if(!_canSpawnInGameObjects.GetValueOrDefault() || _remainingSymbolCount >= SequenceSelector.symbolCount || !IsHost) return;
                
                if(_pendingSymbolPositionsQueue.Count > 0)
                {
                    _symbolSpawnTimer -= Time.deltaTime;
                    
                    if(_symbolSpawnTimer < 0)
                    {
                        _symbolSpawnTimer = 0.02f;
                        SpawnNewSymbol();
                        
                        if(_remainingSymbolCount >= SequenceSelector.symbolCount) _canSpawnInGameObjects = false;
                    }
                }
            }

            void SpawnNewSymbol()
            {
                int index = SequenceSelector.symbolCount - _pendingSymbolPositionsQueue.Count;
                Vector3 pendingPos = _pendingSymbolPositionsQueue.Dequeue();
                var symbolObj = Instantiate(symbolObjectPrefab);
                symbolObj.NetworkObject.Spawn();
                symbolObj.name = "Symbol" + index;
                symbolObj.SetParentAndPosition_Server(symbolContainerInstance.NetworkObject , pendingPos);
                symbolObj.SetSymbolIndex_Server(sequenceSelector.GetNextSymbol(index));
                _remainingSymbolCount++;
            }
        }
        
        public void OnPlayerInput(ulong playerId , SymbolObject selectedSymbol)
        {
            if(selectedSymbol.Clicked) return;

            if(sequenceSelector.ConfirmSymbolCorrect(playerId , selectedSymbol.SymbolIndex))
            {
                selectedSymbol.ClickedSequence_ServerRpc(playerId);
                scorer.ScoreSuccess(playerId);
                OnSymbolDeactivated();
            }
            else
                scorer.ScoreFailure(playerId);
        }

        void OnSymbolDeactivated()
        {
            if(--_remainingSymbolCount <= 0) WaitForEndingSequence_ClientRpc();
        }
        
        [ClientRpc]
        private void WaitForEndingSequence_ClientRpc()
        {
            scorer.OnGameEnd();
            introOutroRunner.DoOutro(EndGame);
        }

        private void EndGame()
        {
            if(IsHost) StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if(IsHost) return;
            
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            _onGameEnd();
        }
    }
}