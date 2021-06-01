using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    /*
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
    */
    
    public string Name { get; set; }

    public int ID { get; set; }

    public int CurrentPosition { get; set; }

    //Vuelta actual
    public int CurrentLap { get; set; }

    //Número total de vueltas
    public int TotalLaps;

    //Vuelta actual que incrementa o decrementa en función de si se ha cruzado la meta correctamente o al revés.
    public int CorrectCurrentLap = 0;

    //La posición del jugador en el circuito, la dirección que debe mirar para seguir adelante y su velocidad
    public Vector3 CurrentCircuitPosition, LookAtPoint, Speed;

    //La dirección que sigue el jugador. Si sigue bien, tendrá valor positivo; si no, tendrá valor negativo.
    //
    public float Direction, ArcInfo;

    //Parte de la línea del recorrido en la que se encuentra un jugador
    public int CurrentSegmentIdx = 0;

    //La distancia total que ha recorrido un jugador.
    public float TotalDistance = 0;

    //Devuelve true si el jugador va marcha atrás y false si no.
    public bool Backwards = false;

    //Devuelve true si ha terminado la carrera y false si no.
    public bool Finished = false;

    private SetupPlayer _setupPlayer;

    private void Awake()
    {
        _setupPlayer = GetComponent<SetupPlayer>();
    }

    public void CrossFinishLine()
    {
        if (!Backwards)
        {
            if (CurrentLap == TotalLaps)
            {
                Finished = true;
                _setupPlayer.StopCar();
            }
            else
            {
                if (CorrectCurrentLap == CurrentLap)
                {
                    CurrentLap++;
                }
                CorrectCurrentLap++;
            }
        }
        else
        {
            CorrectCurrentLap--;
        }
    }

    public override string ToString()
    {
        return Name;
    }
}