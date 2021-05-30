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

    private MyNetworkManager m_NetworkManager;

    [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField inputFieldIP;
    [SerializeField] private InputField inputFieldName;


    [Header("In-Game HUD")] [SerializeField]
    private GameObject inGameHUD;

    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textLaps;
    [SerializeField] private Text textPosition;
    [SerializeField] private Text textWrongDirection;

    private void Awake()
    {
        m_NetworkManager = FindObjectOfType<MyNetworkManager>();
    }

    private void Start()
    {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        ActivateMainMenu();
    }

    public void UpdateSpeed(int speed)
    {
        textSpeed.text = "Speed " + speed + " Km/h";
    }

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
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

    //Función que actualiza el texto para indicar la dirección de vuelta errónea
    public void UpdateUIDirectionMessage(string text)
    {
        textWrongDirection.text = text;
    }


}