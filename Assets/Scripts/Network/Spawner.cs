using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPlayer _playerPrefab;

    //Other compoents
    private CharacterInputHandler _characterInputHandler;
    // Mapping between Token ID and Re-created Players
    private Dictionary<int, NetworkPlayer> _mapTokenIDWithNetworkPlayer;
    // Start is called before the first frame update
    
    void Awake()
    {
        //Create a new Dictionary
        _mapTokenIDWithNetworkPlayer = new Dictionary<int, NetworkPlayer>();
    }

    int GetPlayerToken(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)
        {
            // Just use the local Player Connection Token
            return ConnectionTokenUtils.HashToken(GameManager.instance.GetConnectionToken());
        }
        else
        {
            // Get the Connection Token stored when the Client connects to this Host
            var token = runner.GetPlayerConnectionToken(player);

            if (token != null)
                return ConnectionTokenUtils.HashToken(token);

            Debug.LogError($"GetPlayerToken returned invalid token");

            return 0; // invalid
        }
    }
    
    public void SetConnectionTokenMapping(int token, NetworkPlayer networkPlayer)
    {
        _mapTokenIDWithNetworkPlayer.Add(token, networkPlayer);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            //Get the token for the player
            int playerToken = GetPlayerToken(runner, player);

            Debug.Log($"OnPlayerJoined we are server. Connection token {playerToken}");

            //Check if the token is already recorded by the server. 
            if (_mapTokenIDWithNetworkPlayer.TryGetValue(playerToken, out NetworkPlayer networkPlayer))
            {
                Debug.Log($"Found old connection token for token {playerToken}. Assigning controlls to that player");

                networkPlayer.GetComponent<NetworkObject>().AssignInputAuthority(player);

                networkPlayer.Spawned();
            }
            else
            {
                Debug.Log($"Spawning new player for connection token {playerToken}");
                NetworkPlayer spawnedNetworkPlayer = runner.Spawn(_playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);

                //Store the token for the player
                spawnedNetworkPlayer.token = playerToken;

                //Store the mapping between playerToken and the spawned network player
                _mapTokenIDWithNetworkPlayer[playerToken] = spawnedNetworkPlayer;
            }
        }
        else Debug.Log("OnPlayerJoined");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (_characterInputHandler == null && NetworkPlayer.Local != null)
            _characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

        if (_characterInputHandler != null)
            input.Set(_characterInputHandler.GetNetworkInput());

    }

    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("OnConnectedToServer"); }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Debug.Log("OnShutdown"); }
    public void OnDisconnectedFromServer(NetworkRunner runner) { Debug.Log("OnDisconnectedFromServer"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectRequest"); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");
        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        FindObjectOfType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    
    public void OnHostMigrationCleanUp()
    {
        Debug.Log("Spawner OnHostMigrationCleanUp started");
       
        foreach (KeyValuePair<int, NetworkPlayer> entry in _mapTokenIDWithNetworkPlayer)
        {
            NetworkObject networkObjectInDictionary = entry.Value.GetComponent<NetworkObject>();

            if (networkObjectInDictionary.InputAuthority.IsNone)
            {
                Debug.Log($"{Time.time} Found player that has not reconnected. Despawning {entry.Value.nickName}");
                networkObjectInDictionary.Runner.Despawn(networkObjectInDictionary);
                
            }
        }
        Debug.Log("Spawner OnHostMigrationCleanUp completed");
    }
}
