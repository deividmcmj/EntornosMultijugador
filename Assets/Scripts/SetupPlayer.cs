using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class SetupPlayer : NetworkBehaviour
{
    [SyncVar] private int _id;
    [SyncVar] private string _name;

    private UIManager _uiManager;
    private MyNetworkManager _networkManager;
    private PlayerController _playerController;
    private PlayerInfo _playerInfo;
    private PolePositionManager _polePositionManager;

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();
        _id = NetworkServer.connections.Count - 1;

    }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        _playerInfo.ID = _id;
        _playerInfo.CurrentLap = 0;
        _polePositionManager.AddPlayer(_playerInfo);
        CmdSetDisplayName(_uiManager.inputName);
        _playerInfo.Name = displayName;
        Debug.Log(message: " [SERVER] Conectado: " + displayName);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    }

    #endregion

    private void Awake()
    {
        _playerInfo = GetComponent<PlayerInfo>();
        _playerController = GetComponent<PlayerController>();
        _networkManager = FindObjectOfType<MyNetworkManager>();
        _polePositionManager = FindObjectOfType<PolePositionManager>();
        _uiManager = FindObjectOfType<UIManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            _playerController.enabled = true;
            _playerController.OnSpeedChangeEvent += OnSpeedChangeEventHandler;
            ConfigureCamera();
        }
    }

    void OnSpeedChangeEventHandler(float speed)
    {
        _uiManager.UpdateSpeed((int) speed * 5); // 5 for visualization purpose (km/h)
    }

    void ConfigureCamera()
    {
        if (Camera.main != null) Camera.main.gameObject.GetComponent<CameraController>().m_Focus = this.gameObject;
    }

    public void UpdateDirectionMessage(string message)
    {
        if (isLocalPlayer)
        {
            _uiManager.UpdateUIDirectionMessage(message);
        }
    }

    //Nombre del jugador, con su getter y setter
    [SyncVar] [SerializeField] private string displayName;

   
    [Server]
    public void SetDisplayName(string newName)
    {
        displayName = newName;
    }

    public string GetDisplayName()
    {
        return displayName;
    }

    [Command]
    public void CmdSetDisplayName(string newDisplayName)
    {
        SetDisplayName(newDisplayName);
    }


    //Color del jugador, con su getter y setter

    [SyncVar] [SerializeField] private Color displayColor;

    [Server]
    public void SetDisplayColor(Color newColor)
    {
        displayColor = newColor;
    }

    public Color GetDisplayColor()
    {
        return displayColor;
    }

    
}