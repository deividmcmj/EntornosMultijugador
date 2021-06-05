using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool showGUI = true;
    public string inputName = null;
    public Color color;

    private MyNetworkManager m_NetworkManager;
    private PolePositionManager m_PolePositionManager;

    [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private Button buttonColor;
    [SerializeField] private InputField inputFieldIP;
    [SerializeField] private InputField inputFieldName;
    [SerializeField] private Image colorCube;


    [Header("In-Game HUD")] [SerializeField] private GameObject inGameHUD;
    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textWrongDirection;


    [Header("Results HUD")] [SerializeField] private GameObject resultsHUD;
    [SerializeField] private Text[] finalResults;

    [Header("Results HUD")] [SerializeField] private GameObject buttonResultsHUD;
    [SerializeField] private Button buttonMenu;


    [Header("Abandon HUD")] [SerializeField] private GameObject abandonHUD;
    [SerializeField] private Text textAbandon;

    private CameraController m_camera;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
        m_PolePositionManager = FindObjectOfType<PolePositionManager>();
        m_camera = FindObjectOfType<CameraController>();

        color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        colorCube.color = color;
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        buttonColor.onClick.AddListener(() => NewColor());
        buttonMenu.onClick.AddListener(() => ActivateMainMenu());

        ActivateMainMenu();
    }

    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    public void UpdateLaps(int laps)
    {
        textLaps.text = "LAP: " + laps + " /3";
    }

    public void UpdatePosition(string position)
    {
        textPosition.text = position;
    }

    public void UpdateFinalPosition(int i, string position)
    {
        finalResults[i].text = position;
        Debug.Log(message: "He llegado: " + i);
    }

    private void ActivateMainMenu()
    {
        m_NetworkManager.StopHost();
        m_NetworkManager.StopClient();
        m_NetworkManager.StopServer();
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        resultsHUD.SetActive(false);
        buttonResultsHUD.SetActive(false);
        abandonHUD.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        resultsHUD.SetActive(false);
        buttonResultsHUD.SetActive(false);
        abandonHUD.SetActive(false);
    }

    private void ActivateResultsHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        resultsHUD.SetActive(true);
        //buttonResultsHUD.SetActive(false);
        abandonHUD.SetActive(false);
    }

    private void ActivateButtonResultsHUD()
    {
        buttonResultsHUD.SetActive(true);
    }

    public void HideButtonResultsHUD()
    {
        buttonResultsHUD.SetActive(false);
    }

    private void ActivateAbandonHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        resultsHUD.SetActive(false);
        buttonResultsHUD.SetActive(false);
        abandonHUD.SetActive(true);
    }

    private void StartHost()
    {
        if(inputFieldName.text != "")
        {
            inputName = inputFieldName.text;
            m_NetworkManager.StartHost();
            ActivateInGameHUD();
        }
        else
        {
            Debug.Log(message: "Debes ponerte un nombre");
        }

    }

    private void StartClient()
    {
        if (inputFieldName.text != "")
        {
            inputName = inputFieldName.text;
            m_NetworkManager.StartClient();
            m_NetworkManager.networkAddress = inputFieldIP.text;
            ActivateInGameHUD();
        }
        else
        {
            Debug.Log(message: "Debes ponerte un nombre");
        }
    }

    private void StartServer()
    {
        m_NetworkManager.StartServer();
        ActivateInGameHUD();
    }

    //Función que cambia el color por uno al azar
    private void NewColor()
    {
        color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        colorCube.color = color;

    }


    //Función que actualiza el texto para indicar la dirección de vuelta errónea
    public void UpdateUIDirectionMessage(string text)
    {
        textWrongDirection.text = text;
    }

    public void FinishRace()
    {
        ActivateResultsHUD();
    }

    public void ShowMenuButton()
    {
        ActivateButtonResultsHUD();
    }

    public void BackToMenu()
    {
        ActivateMainMenu();
    }

    public void AbandonMenu()
    {
        ActivateAbandonHUD();
    }
}