namespace Utils
{
    using Unity.Netcode;
    using UnityEngine;
    
    public class PlayerData : INetworkSerializable
    {
        public string Name;
        public ulong Id;
        public int Score;
        
        public PlayerData(string name , ulong id , int score = 0) { this.Name = name; this.Id = id; this.Score = score; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Score);
        }
    }
}