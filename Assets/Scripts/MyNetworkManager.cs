using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public PolePositionManager _polePositionManager;
    public override void Awake()
    {
        if (_polePositionManager == null)
        {
            _polePositionManager = FindObjectOfType<PolePositionManager>();
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            Debug.Log(message: "Un jugador se ha desconectado");
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());
        }

        NetworkIdentity.Destroy(GameObject.Find($"Player [connId={conn.connectionId}]"));
        base.OnServerDisconnect(conn);
    }



    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            Debug.Log(message: "El servidor se ha desconectado");
            _polePositionManager._uiManager.BackToMenu();
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());
        }
        base.OnClientDisconnect(conn);
    }
}
