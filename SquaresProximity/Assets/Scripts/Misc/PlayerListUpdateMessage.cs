namespace Misc
{
    using Unity.Netcode;
    
    public struct PlayerListUpdateMessage : INetworkSerializable
    {
        public string PlayerList;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerList);
        }
    }
}