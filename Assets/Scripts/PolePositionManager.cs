using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using Mirror.Cloud.Examples.Pong;
using UnityEngine;
using System.Threading;

public class PolePositionManager : NetworkBehaviour
{
    public int maxNumPlayers = 4;
    public int numPlayers;
    public string previousRaceOrder = "";
    private MyNetworkManager _networkManager;

    public Mutex mutex = new Mutex();

    private readonly List<PlayerInfo> _players = new List<PlayerInfo>(4);
    private List<PlayerInfo> current_players = new List<PlayerInfo>(4);

    private CircuitController _circuitController;
    private readonly List<GameObject> _debuggingSpheres = new List<GameObject>(4);

    public event Action<string> OnPositionChangeEvent;
    public event Action<int, string> OnFinalPositionChangeEvent;
    public event Action<string> OnCountdownChangeEvent;

    public UIManager _uiManager;
    public SetupPlayer _setupPlayer;


    private int readyPlayers = 0;
    private int finishedPlayers = 0;
    private float countDown = 0;

    public bool AllReady
    {
        get
        {
            return readyPlayers == _players.Count;
        }
    }
    public int GetReadyPlayers()
    {
        return readyPlayers;
    }
    public void SetReadyPlayers(int value)
    {
        readyPlayers = value;
    }

    public bool AllFinished
    {
        get
        {
            return finishedPlayers == _players.Count;
        }
    }
    public int GetFinishedPlayers()
    {
        return finishedPlayers;
    }
    public void SetFinishedPlayers(int value)
    {
        finishedPlayers = value;
    }


    public List<PlayerInfo> GetPlayers()
    {
        return _players;
    }

    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        if (_uiManager == null) _uiManager = FindObjectOfType<UIManager>();
        if (_setupPlayer == null) _setupPlayer = FindObjectOfType<SetupPlayer>();
    }

    private void Update()
    {
        if (_players.Count == 0)
            return;

        UpdateRaceProgress();
            
    }

    public void AddPlayer(PlayerInfo player)
    {
        //Creamos un jugador y su correspondiente esfera en el circuito
        _players.Add(player);
        _debuggingSpheres.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        _debuggingSpheres[player.ID].GetComponent<SphereCollider>().enabled = false;

        Debug.Log(message: "Ahora hay " + _players.Count + " jugadores");

        if (_players.Count >= 2)
        {
            _uiManager.ShowStartButton();
        }
    }

    public void RemovePlayer(PlayerInfo player)
    {
        //Destroy(_debuggingSpheres[player.ID]);
        //_debuggingSpheres.Remove(_debuggingSpheres[player.ID]);
        _players.Remove(player);

        Debug.Log(message: "Ahora hay " + _players.Count + " jugadores");

        finishedPlayers--;
    }

    public void UpdateRaceProgress()
    {
        if (AllReady)
        {
            Debug.Log(message: "Todos readys");

            UpdateCountdown();
            if (countDown >= 3.0f)
            {
                foreach (PlayerInfo player in _players)
                {
                    player.GetComponent<SetupPlayer>().StartCar();
                }
            }
        }
        // Update car arc-lengths
        foreach (PlayerInfo player in _players)
        {
            ComputeCarArcLength(player.ID);
            if (player.CorrectCurrentLap == 0)
            {
                player.TotalDistance = player.ArcInfo - _circuitController.CircuitLength;
            }
            else
            {
                player.TotalDistance = _circuitController.CircuitLength * (player.CorrectCurrentLap - 1) + player.ArcInfo;
            }
            //Debug.LogFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", readyPlayers, player.CorrectCurrentLap, player.CurrentLap, player.GetFinished(), player.GetReady(), finishedPlayers, player.FinalPosition, countDown);
        }
        
        _players.Sort(delegate(PlayerInfo p, PlayerInfo q)
        {
            if (p.TotalDistance < q.TotalDistance)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        string myRaceOrder = "";

        foreach (PlayerInfo player in _players)
        {
            player.CurrentPosition = _players.IndexOf(player) + 1;
            myRaceOrder += player.Name + " ";
        }

        if(!previousRaceOrder.Equals(myRaceOrder))
        {
            if (OnPositionChangeEvent != null)
            {
                OnPositionChangeEvent(GetPositionPlayers());
            }
            
            previousRaceOrder = myRaceOrder;
        }

        if (AllFinished)
        {
            _uiManager.ShowMenuButton();
            //finishedPlayers = -1;
        }

        if (finishedPlayers < 0)
        {
            _uiManager.AbandonMenu();
        }
    }

    public string GetPositionPlayers()
    {
        string myRaceOrder = "";
        int i = 0;
        foreach (PlayerInfo player in _players)
        {
            i++;
            myRaceOrder += i + ". " + player.Name + "\n";
        }

        return myRaceOrder;
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = _players[id].transform.position;

        int segIdx;
        float carDist;
        Vector3 carProj;
        //Dirección del jugador
        Vector3 carDir;

        float minArcL =
            _circuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDir, out carDist);

        _debuggingSpheres[id].transform.position = carProj;

        //Actualizamos la posición, la rotación y la dirección del jugador
        _players[id].CurrentCircuitPosition = carProj;
        _players[id].LookAtPoint = carDir;
        _players[id].Direction = Vector3.Dot((carDir - carProj).normalized, _players[id].Speed.normalized);
        _players[id].ArcInfo = minArcL;
        _players[id].CurrentSegmentIdx = segIdx;

        return minArcL;
    }

    public void UpdateCountdown()
    {
        countDown += Time.deltaTime;
        if (countDown >= 0.0f && countDown < 1.0f)
        {
            OnCountdownChangeEvent("3");
        }
        else if (countDown >= 1.0f && countDown < 2.0f)
        {
            OnCountdownChangeEvent("2");
        }
        else if (countDown >= 2.0f && countDown < 3.0f)
        {
            OnCountdownChangeEvent("1");
        }
        else if (countDown >= 3.0f && countDown < 4.0f)
        {
            OnCountdownChangeEvent("YA!");
        }
        else
        {
            OnCountdownChangeEvent("");
        }
    }

    public void PlayerFinished(PlayerInfo player)
    {
        OnFinalPositionChangeEvent(finishedPlayers - 1, finishedPlayers + ": " + player.Name);
    }
}