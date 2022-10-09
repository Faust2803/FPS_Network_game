using UnityEngine;
using Fusion;

public class NetworkInGameMessages : NetworkBehaviour
{
     private InGameMessagesUIHander _inGameMessagesUIHander;
    
    public void SendInGameRPCMessage(string userNickName, string message)
    {
        RPC_InGameMessage($"<b>{userNickName}</b> {message}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");

        if (_inGameMessagesUIHander == null)
            _inGameMessagesUIHander = NetworkPlayer.Local.LocalCameraHandler.GetComponentInChildren<InGameMessagesUIHander>();

        if (_inGameMessagesUIHander != null)
            _inGameMessagesUIHander.OnGameMessageReceived(message);
    }
}
