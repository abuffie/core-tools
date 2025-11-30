using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aarware.Services.Networking {
    /// <summary>
    /// Service for managing multiplayer networking across different platforms.
    /// Uses an event-driven pattern for network communication.
    /// </summary>
    public class NetworkingService : ServiceBase<INetworkingProvider> {
        public NetworkConnectionState ConnectionState => currentProvider?.ConnectionState ?? NetworkConnectionState.Disconnected;
        public NetworkPlayer LocalPlayer => currentProvider?.LocalPlayer;
        public NetworkRoom CurrentRoom => currentProvider?.CurrentRoom;

        // Connection Events
        public event Action OnConnectedToMaster;
        public event Action OnDisconnected;
        public event Action<string> OnConnectionFailed;

        // Lobby Events
        public event Action OnJoinedLobby;
        public event Action OnLeftLobby;
        public event Action<List<NetworkRoom>> OnRoomListUpdate;

        // Room Events
        public event Action OnJoinedRoom;
        public event Action OnLeftRoom;
        public event Action<string> OnJoinRoomFailed;
        public event Action<NetworkPlayer> OnPlayerJoined;
        public event Action<NetworkPlayer> OnPlayerLeft;
        public event Action<NetworkPlayer> OnMasterClientSwitched;

        // Network Events
        public event Action<byte, object[], NetworkPlayer> OnNetworkEvent;

        public override void SetProvider(INetworkingProvider provider) {
            UnsubscribeFromProvider();
            base.SetProvider(provider);
            SubscribeToProvider();
        }

        void SubscribeToProvider() {
            if (currentProvider == null) {
                return;
            }

            currentProvider.OnConnectedToMaster += () => OnConnectedToMaster?.Invoke();
            currentProvider.OnDisconnected += () => OnDisconnected?.Invoke();
            currentProvider.OnConnectionFailed += (error) => OnConnectionFailed?.Invoke(error);
            currentProvider.OnJoinedLobby += () => OnJoinedLobby?.Invoke();
            currentProvider.OnLeftLobby += () => OnLeftLobby?.Invoke();
            currentProvider.OnRoomListUpdate += (rooms) => OnRoomListUpdate?.Invoke(rooms);
            currentProvider.OnJoinedRoom += () => OnJoinedRoom?.Invoke();
            currentProvider.OnLeftRoom += () => OnLeftRoom?.Invoke();
            currentProvider.OnJoinRoomFailed += (error) => OnJoinRoomFailed?.Invoke(error);
            currentProvider.OnPlayerJoined += (player) => OnPlayerJoined?.Invoke(player);
            currentProvider.OnPlayerLeft += (player) => OnPlayerLeft?.Invoke(player);
            currentProvider.OnMasterClientSwitched += (newMaster) => OnMasterClientSwitched?.Invoke(newMaster);
            currentProvider.OnNetworkEvent += (code, data, sender) => OnNetworkEvent?.Invoke(code, data, sender);
        }

        void UnsubscribeFromProvider() {
            // Note: In a production implementation, you'd store references to the event handlers
            // to properly unsubscribe. For simplicity, this is a basic implementation.
        }

        public async Task<ServiceResult> ConnectAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.ConnectAsync();
        }

        public async Task<ServiceResult> DisconnectAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.DisconnectAsync();
        }

        public async Task<ServiceResult> JoinLobbyAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.JoinLobbyAsync();
        }

        public async Task<ServiceResult> LeaveLobbyAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.LeaveLobbyAsync();
        }

        public async Task<ServiceResult> CreateRoomAsync(string roomName, int maxPlayers, Dictionary<string, object> customProperties = null) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.CreateRoomAsync(roomName, maxPlayers, customProperties);
        }

        public async Task<ServiceResult> JoinRoomAsync(string roomName) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.JoinRoomAsync(roomName);
        }

        public async Task<ServiceResult> JoinRandomRoomAsync(Dictionary<string, object> expectedProperties = null) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.JoinRandomRoomAsync(expectedProperties);
        }

        public async Task<ServiceResult> LeaveRoomAsync() {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.LeaveRoomAsync();
        }

        public async Task<ServiceResult<List<NetworkRoom>>> GetRoomListAsync() {
            if (!IsInitialized) {
                return ServiceResult<List<NetworkRoom>>.Failed("Networking service not initialized");
            }
            return await currentProvider.GetRoomListAsync();
        }

        public List<NetworkPlayer> GetPlayersInRoom() {
            if (!IsInitialized) {
                return new List<NetworkPlayer>();
            }
            return currentProvider.GetPlayersInRoom();
        }

        public async Task<ServiceResult> SendNetworkEventAsync(byte eventCode, object[] data, NetworkEventOptions options = null) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.SendNetworkEventAsync(eventCode, data, options);
        }

        public async Task<ServiceResult> SetPlayerPropertiesAsync(Dictionary<string, object> properties) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.SetPlayerPropertiesAsync(properties);
        }

        public async Task<ServiceResult> SetRoomPropertiesAsync(Dictionary<string, object> properties) {
            if (!IsInitialized) {
                return ServiceResult.Failed("Networking service not initialized");
            }
            return await currentProvider.SetRoomPropertiesAsync(properties);
        }

        public override void Shutdown() {
            UnsubscribeFromProvider();
            base.Shutdown();
        }
    }
}
