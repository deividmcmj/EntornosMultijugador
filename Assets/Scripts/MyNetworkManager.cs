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

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());
        }
        base.OnServerDisconnect(conn);
    }
  

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        _polePositionManager._uiManager.AbandonMenu();
        _polePositionManager._setupPlayer.StopAllCars();
        base.OnClientDisconnect(conn);
    }
}
