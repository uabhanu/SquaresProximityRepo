namespace Utils.OnlineMultiplayer
{
    using TMPro;
    using Unity.Netcode;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// You already have a ScoreManager so you will need to move the functions to that class
    /// </summary>
    public class Scorer : NetworkBehaviour
    {
        private ulong _localId;

        [SerializeField] NetworkedDataStore dataStore;
        [SerializeField] TMP_Text scoreTMPText = default;

        [Tooltip(
            "When the game ends, this will be called once for each player in order of rank (1st-place first, and so on).")]
        [SerializeField]
        UnityEvent<PlayerData> onGameEnd;

        [ClientRpc]
        private void UpdateScoreOutput_ClientRpc(ulong id , int score)
        {
            if(_localId == id) scoreTMPText.text = score.ToString("00");
        }

        public override void OnNetworkSpawn()
        {
            _localId = NetworkManager.Singleton.LocalClientId;
        }

        public void OnGameEnd()
        {
            dataStore.GetAllPlayerData(onGameEnd);
        }

        public void ScoreFailure(ulong id)
        {
            int newScore = dataStore.UpdateScore(id , -1);
            UpdateScoreOutput_ClientRpc(id , newScore);
        }

        public void ScoreSuccess(ulong id)
        {
            int newScore = dataStore.UpdateScore(id , 1);
            UpdateScoreOutput_ClientRpc(id , newScore);
        }
    }
}
