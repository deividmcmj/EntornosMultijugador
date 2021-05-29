using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        PlayerInfo player = conn.identity.GetComponent<PlayerInfo>();
        player.SetDisplayName("Player " + numPlayers);
        player.SetDisplayColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

        Debug.Log(message: " [SERVER] Conectado " + player.GetDisplayName());
        Debug.Log(message: " [SERVER] Con color " + player.GetDisplayColor());

    }

}
