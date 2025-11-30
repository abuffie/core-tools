using System;
using System.Collections.Generic;

namespace Aarware.Services.Networking {
    /// <summary>
    /// Represents a network event/message that can be sent between clients.
    /// Similar to Photon's RaiseEvent pattern.
    /// </summary>
    [Serializable]
    public class NetworkEvent {
        public byte eventCode;
        public object[] data;
        public NetworkEventOptions options;

        public NetworkEvent(byte eventCode, object[] data, NetworkEventOptions options = null) {
            this.eventCode = eventCode;
            this.data = data;
            this.options = options ?? new NetworkEventOptions();
        }
    }

    /// <summary>
    /// Options for sending network events.
    /// </summary>
    [Serializable]
    public class NetworkEventOptions {
        public NetworkEventReceivers receivers;
        public bool reliable;

        public NetworkEventOptions() {
            receivers = NetworkEventReceivers.All;
            reliable = true;
        }
    }

    /// <summary>
    /// Who should receive network events.
    /// </summary>
    public enum NetworkEventReceivers {
        All,
        Others,
        MasterClient,
        Specific
    }

    /// <summary>
    /// Represents a player in the network session.
    /// </summary>
    [Serializable]
    public class NetworkPlayer {
        public string playerId;
        public string playerName;
        public bool isMasterClient;
        public bool isLocal;
        public Dictionary<string, object> customProperties;

        public NetworkPlayer(string playerId, string playerName, bool isLocal = false) {
            this.playerId = playerId;
            this.playerName = playerName;
            this.isMasterClient = false;
            this.isLocal = isLocal;
            this.customProperties = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Represents a network room/lobby.
    /// </summary>
    [Serializable]
    public class NetworkRoom {
        public string roomId;
        public string roomName;
        public int maxPlayers;
        public int currentPlayerCount;
        public bool isOpen;
        public bool isVisible;
        public Dictionary<string, object> customProperties;

        public NetworkRoom(string roomId, string roomName, int maxPlayers) {
            this.roomId = roomId;
            this.roomName = roomName;
            this.maxPlayers = maxPlayers;
            this.currentPlayerCount = 0;
            this.isOpen = true;
            this.isVisible = true;
            this.customProperties = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Connection state for networking.
    /// </summary>
    public enum NetworkConnectionState {
        Disconnected,
        Connecting,
        Connected,
        ConnectedToMaster,
        JoiningLobby,
        InLobby,
        JoiningRoom,
        InRoom,
        Disconnecting
    }
}
