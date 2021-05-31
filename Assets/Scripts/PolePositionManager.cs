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

    private readonly List<PlayerInfo> _players = new List<PlayerInfo>(4);
    private CircuitController _circuitController;
    private readonly List<GameObject> _debuggingSpheres = new List<GameObject>(4);

    //En lugar de usar listas y arrays, usamos diccionarios que guarden como clave el ID del jugador

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
        if (_players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(PlayerInfo player)
    {
        //_players.Add(player);
        //Creamos un jugador y su correspondiente esfera en el circuito
        _players.Add(player);
        _debuggingSpheres.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        _debuggingSpheres[player.ID].GetComponent<SphereCollider>().enabled = false;
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
        /*
        for (int i = 0; i < _players.Count; ++i)
        {
            arcLengths[i] = ComputeCarArcLength(i);
        }
        */
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
            myRaceOrder += player.Name + " ";
        }

        Debug.Log("El orden de carrera es: " + myRaceOrder);
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

        //this._debuggingSpheres[id].transform.position = carProj;
        _debuggingSpheres[id].transform.position = carProj;

        /*
        if (_Players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_Players[id].CurrentLap - 1);
        }
        */

        //Actualizamos la posición, la rotación y la dirección del jugador
        _players[id].CurrentCircuitPosition = carProj;
        _players[id].LookAtPoint = carDir;
        _players[id].Direction = Vector3.Dot((carDir - carProj).normalized, _players[id].Speed.normalized);
        _players[id].ArcInfo = minArcL;
        _players[id].CurrentSegmentIdx = segIdx;

        return minArcL;
    }
}