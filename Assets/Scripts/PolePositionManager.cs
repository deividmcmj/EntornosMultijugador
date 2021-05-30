using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror;
using Mirror.Cloud.Examples.Pong;
using UnityEngine;

public class PolePositionManager : NetworkBehaviour
{
    public int numPlayers;
    private MyNetworkManager _networkManager;

    //private readonly List<PlayerInfo> _players = new List<PlayerInfo>(4);
    private CircuitController _circuitController;
    //private GameObject[] _debuggingSpheres;

    //En lugar de usar listas y arrays, usamos diccionarios que guarden como clave el ID del jugador
    private readonly Dictionary<int, PlayerInfo> _Players = new Dictionary<int, PlayerInfo>(4);
    private readonly Dictionary<int, GameObject> _DebbuggingSpheres = new Dictionary<int, GameObject>(4);

    private void Awake()
    {
        if (_networkManager == null) _networkManager = FindObjectOfType<MyNetworkManager>();
        if (_circuitController == null) _circuitController = FindObjectOfType<CircuitController>();
        //En vez de crear tantas esferas como posibles conexiones puedan haber, se crean tantas esferas como
        //jugadores conectados hayan
        /*
        _debuggingSpheres = new GameObject[_networkManager.maxConnections];
        for (int i = 0; i < _networkManager.maxConnections; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }
        */
    }

    private void Update()
    {
        if (_Players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(PlayerInfo player)
    {
        //_players.Add(player);
        //Creamos un jugador y su correspondiente esfera en el circuito
        _Players.Add(player.ID, player);
        _DebbuggingSpheres.Add(player.ID, GameObject.CreatePrimitive(PrimitiveType.Sphere));
        _DebbuggingSpheres[player.ID].GetComponent<SphereCollider>().enabled = false;
    }

    private class PlayerInfoComparer : Comparer<PlayerInfo>
    {
        float[] _arcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            _arcLengths = arcLengths;
        }

        public override int Compare(PlayerInfo x, PlayerInfo y)
        {
            if (_arcLengths[x.ID] < _arcLengths[y.ID])
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        /*
        float[] arcLengths = new float[_players.Count];
        for (int i = 0; i < _players.Count; ++i)
        {
            arcLengths[i] = ComputeCarArcLength(i);
        }
        */
        foreach (PlayerInfo player in _Players.Values)
        {
            ComputeCarArcLength(player.ID);
        }

        //Se crea un array con los valores del diccionario de jugadores, y se ordena en función de quién ha
        //recorrido mayor distancia
        PlayerInfo[] players = _Players.Values.ToArray();
        Array.Sort(players, (p, q) => {
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
        foreach (PlayerInfo player in _Players.Values)
        {
            myRaceOrder += player.Name + " ";
        }

        Debug.Log("El orden de carrera es: " + myRaceOrder);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = _Players[id].transform.position;

        int segIdx;
        float carDist;
        Vector3 carProj;
        //Dirección del jugador
        Vector3 carDir;

        float minArcL =
            _circuitController.ComputeClosestPointArcLength(carPos, out segIdx, out carProj, out carDir, out carDist);

        //this._debuggingSpheres[id].transform.position = carProj;
        _DebbuggingSpheres[id].transform.position = carProj;

        if (_Players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_Players[id].CurrentLap - 1);
        }

        //Actualizamos la posición, la rotación y la dirección del jugador
        _Players[id].CurrentCircuitPosition = carProj;
        _Players[id].LookAtPoint = carDir;
        _Players[id].Direction = Vector3.Dot((carDir - carProj).normalized, this._Players[id].Speed.normalized);

        return minArcL;
    }
}