namespace Interfaces
{
    public interface ILobbyManager
    {
        void CreateLobby(int maxPlayers);
        void JoinLobby(int lobbyId);
        void LeaveLobby();
    }
}