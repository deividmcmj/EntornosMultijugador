using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
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

    public string Name { get; set; }

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


    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    //Vuelta actual
    public int CurrentLap { get; set; }

    //Vuelta actual que incrementa o decrementa en función de si se ha cruzado la meta correctamente o al revés
    public int CorrectCurrentLap;

    //La posición del jugador en el circuito, la dirección que debe mirar para seguir adelante y su velocidad
    public Vector3 CurrentCircuitPosition, LookAtPoint, Speed;

    //La dirección que sigue el jugador. Si sigue bien, tendrá valor positivo; si no, tendrá valor negativo.
    //
    public float Direction, ArcInfo;

    //La distancia total que ha recorrido un jugador.
    public float TotalDistance = 0;

    public override string ToString()
    {
        return Name;
    }
}