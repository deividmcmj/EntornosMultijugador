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
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        SetupPlayer player = conn.identity.GetComponent<SetupPlayer>();
        //player.SetDisplayColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

        Debug.Log(message: " [SERVER] Con color " + player.GetDisplayColor());

    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            _polePositionManager.RemovePlayer(conn.identity.GetComponent<PlayerInfo>());
        }
        base.OnServerDisconnect(conn);
    }
}
