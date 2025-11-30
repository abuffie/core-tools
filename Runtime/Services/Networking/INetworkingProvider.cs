using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aarware.Services.Networking {
    /// <summary>
    /// Interface for platform-specific networking providers.
    /// Follows an event-driven pattern similar to Photon.
    /// </summary>
    public interface INetworkingProvider : IServiceProvider {
        /// <summary>
        /// The networking platform this provider supports.
        /// </summary>
        NetworkingPlatform Platform { get; }

        /// <summary>
        /// Current connection state.
        /// </summary>
        NetworkConnectionState ConnectionState { get; }

        /// <summary>
        /// Current player in the session.
        /// </summary>
        NetworkPlayer LocalPlayer { get; }

        /// <summary>
        /// Current room if in one.
        /// </summary>
        NetworkRoom CurrentRoom { get; }

        // Connection Events
        event Action OnConnectedToMaster;
        event Action OnDisconnected;
        event Action<string> OnConnectionFailed;

        // Lobby Events
        event Action OnJoinedLobby;
        event Action OnLeftLobby;
        event Action<List<NetworkRoom>> OnRoomListUpdate;

        // Room Events
        event Action OnJoinedRoom;
        event Action OnLeftRoom;
        event Action<string> OnJoinRoomFailed;
        event Action<NetworkPlayer> OnPlayerJoined;
        event Action<NetworkPlayer> OnPlayerLeft;
        event Action<NetworkPlayer> OnMasterClientSwitched;

        // Network Events
        event Action<byte, object[], NetworkPlayer> OnNetworkEvent;

        /// <summary>
        /// Connects to the network service.
        /// </summary>
        Task<ServiceResult> ConnectAsync();

        /// <summary>
        /// Disconnects from the network service.
        /// </summary>
        Task<ServiceResult> DisconnectAsync();

        /// <summary>
        /// Joins or creates a lobby.
        /// </summary>
        Task<ServiceResult> JoinLobbyAsync();

        /// <summary>
        /// Leaves the current lobby.
        /// </summary>
        Task<ServiceResult> LeaveLobbyAsync();

        /// <summary>
        /// Creates a new room.
        /// </summary>
        Task<ServiceResult> CreateRoomAsync(string roomName, int maxPlayers, Dictionary<string, object> customProperties = null);

        /// <summary>
        /// Joins an existing room.
        /// </summary>
        Task<ServiceResult> JoinRoomAsync(string roomName);

        /// <summary>
        /// Joins a random available room.
        /// </summary>
        Task<ServiceResult> JoinRandomRoomAsync(Dictionary<string, object> expectedProperties = null);

        /// <summary>
        /// Leaves the current room.
        /// </summary>
        Task<ServiceResult> LeaveRoomAsync();

        /// <summary>
        /// Gets the list of available rooms.
        /// </summary>
        Task<ServiceResult<List<NetworkRoom>>> GetRoomListAsync();

        /// <summary>
        /// Gets the list of players in the current room.
        /// </summary>
        List<NetworkPlayer> GetPlayersInRoom();

        /// <summary>
        /// Sends a network event to other players.
        /// </summary>
        Task<ServiceResult> SendNetworkEventAsync(byte eventCode, object[] data, NetworkEventOptions options = null);

        /// <summary>
        /// Sets custom properties for the local player.
        /// </summary>
        Task<ServiceResult> SetPlayerPropertiesAsync(Dictionary<string, object> properties);

        /// <summary>
        /// Sets custom properties for the current room.
        /// </summary>
        Task<ServiceResult> SetRoomPropertiesAsync(Dictionary<string, object> properties);
    }
}
