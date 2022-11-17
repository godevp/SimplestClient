using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.IO;

public struct Ident
{
    public const string LoginApproved = "1";
    public const string LoginDenied = "2";
    public const string Player = "3";
    public const string SecondPlayer = "4";
    public const string Spectator = "5";
    public const string ChatMsg = "6";
    public const string ObsUpdateX = "7";
    public const string ObsUpdateO = "8";
    public const string PlayerUpdateX = "9";
    public const string PlayerUpdateO = "10";
    public const string winner = "11";
    public const string loser = "12";
    public const string tie = "13";
    public const string turn1 = "14";
    public const string turn2 = "15";
    public const string restart = "16";
    public const string stopWatching = "17";
    public const string exit = "18";
    public const string requestRestart = "19";
    public const string move = "20";
    public const string dscnt = "21";
    public const string room = "22";
    public const string reg = "23";
    public const string logIn = "24";
    public const string FeelTheListOfReplays = "25";
    public const string RequestForReplay = "26";
}


public class NetworkedClient : MonoBehaviour
{
    public static NetworkedClient instace;
    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;
    GameObject logButton;
    GameObject registerButton;
    public GameObject AccountPart;
    public GameObject LoginPart;
    public GameObject GamePart;
    public GameObject stopWatching;
    [SerializeField]
    private TMP_InputField logField;


    public GameObject chat;
    [SerializeField]
    private TMP_InputField passwordField;
    [SerializeField]
    private TMP_InputField NewRoomField;
    [SerializeField]
    private TMP_InputField textField;
    [SerializeField]
    private GameObject prefabText;
    [SerializeField]
    private GameObject prefabForReplay;
    [SerializeField]
    private GameObject gridForReplay;

    [SerializeField]
    private GameObject GridForTexts;
    [SerializeField]
    private TMP_Text ErrorText;
    public Slot _slot;
    public TMP_Text loserORwinner;

    public List<string> ListOfReplays;

    public bool turn = true; // true = first player(X), false = second player(O)
    public bool canMove = false;


    string login;
    string password;
    // Start is called before the first frame update
    void Start()
    {
        instace = this;
        Connect();
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        ListOfReplays = new List<string>();
        foreach (GameObject obj in allGameObjects)
        {
            switch (obj.name)
            {
                case "Login_b":

                    logButton = obj;
                    break;
                case "Register_b":
                
                    registerButton = obj;
                    break;
                case "AccountPart":
                    
                    AccountPart = obj;
                    break;
                case "LoginPart":
                  
                    LoginPart = obj;
                    break;
                case "GamePart":

                    GamePart = obj;
                    break;

                default:
                    break;
            }

        }


        logButton.GetComponent<Button>().onClick.AddListener(LogInF);
        registerButton.GetComponent<Button>().onClick.AddListener(RegisterF);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }

    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);
            //192.168.0.156 home 10.0.254.6
            connectionID = NetworkTransport.Connect(hostID, "192.168.0.156", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }

    public void Disconnect()
    {
       
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }

    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        string[] splitter = msg.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        switch (splitter[0])
        {
            case Ident.LoginApproved:
                GameManager._instance.UpdateGameState(GameState.accountState);
                break;

            case Ident.LoginDenied:
                ErrorText.gameObject.SetActive(true);
                ErrorText.text = "Something wrong or somebody using the account";
                break;

            case Ident.Player:
                GameManager._instance.UpdateGameState(GameState.gameState);
                ActivationForPlayer();
                break;

            case Ident.Spectator:
                GameManager._instance.UpdateGameState(GameState.gameState);
                DeactivateForSpectator();
                break;

            case Ident.ChatMsg:

                var newText = Instantiate(prefabText, GridForTexts.transform);
                newText.GetComponent<TMP_Text>().text = splitter[1];
                break;

            case Ident.ObsUpdateX:
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Xsprite;

                break;
            case Ident.ObsUpdateO:
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Osprite;
                break;

            case Ident.PlayerUpdateX:
                //set the sprite to X in the button[splitter[1]]
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Xsprite;

                if (turn)
                {
                    canMove = false;
                    loserORwinner.text = "";
                }
                else
                {
                    loserORwinner.text = "Your Move";
                    canMove = true;
                }

                break;

            case Ident.PlayerUpdateO:
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Osprite;
                if (!turn)
                {
                    canMove = false;
                    loserORwinner.text = "";
                }
                else
                {
                    loserORwinner.text = "Your Move";
                    canMove = true;
                }
                break;

            case Ident.winner:  //win 
                canMove = false;
                loserORwinner.text = "Won";
                break;

            case Ident.loser://lose
                canMove = false;
                loserORwinner.text = "Lost";
                break;

            case Ident.tie://tie
                canMove = false;
                loserORwinner.text = "NO WINNER";
                break;

            case Ident.turn1:
                canMove = true;
                turn = true;//means we Xplayer
                loserORwinner.text = "Your Move";
                break;

            case Ident.turn2:
                canMove = false;
                turn = false;//means we Oplayer
                break;


            case Ident.restart:
                _slot.EmptyButtons();
                canMove = false;
                loserORwinner.text = "";
                break;

            case Ident.FeelTheListOfReplays:
                if (splitter[1] == "clean")
                {
                    ListOfReplays.Clear();
                }
               
                //foreach of these create a replay button and fill the who and where Moved.
                else if (splitter[1] == "done")
                {
                    foreach (string _rep in ListOfReplays)
                    {
                        Debug.Log(_rep);
                        var newRep = Instantiate(prefabForReplay, gridForReplay.transform);
                        newRep.GetComponent<ReplayButton>().text.text = _rep;
                    }
                }
                else if(splitter[1] != "done" && splitter[1] != "clean")
                {
                    ListOfReplays.Add(splitter[1]);
                }
                break;
            case Ident.RequestForReplay:
                if(splitter[1] == "1")
                {
                    _slot.buttons[int.Parse(splitter[2])].GetComponent<Image>().sprite = _slot.Xsprite;
                }
                if (splitter[1] == "2")
                {
                    _slot.buttons[int.Parse(splitter[2])].GetComponent<Image>().sprite = _slot.Osprite;
                }
                if(splitter[1] == "obsExit")
                {
                    stopWatching.gameObject.SetActive(true);
                }

                
                break;
          


            default:
                break;
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
    public void LogInF()
    {
        if(logField.text.Length != 0)
            login = logField.text;
        if (passwordField.text.Length != 0)
            password = passwordField.text;

        if(logField.text.Length != 0 && passwordField.text.Length != 0)
        {
            SendMessageToHost(Ident.logIn + ',' + login + ',' +                              
                              password);
        }
    }
    public void RegisterF()
    {
        if (logField.text.Length != 0)
            login = logField.text;
        if (passwordField.text.Length != 0)
            password = passwordField.text;


        if (logField.text.Length != 0 && passwordField.text.Length != 0)
        {
            SendMessageToHost(Ident.reg + ',' + login + ',' +
                              password);
        }
    }
    public void CreateRoom()
    {
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(Ident.room + ',' + NewRoomField.text);
        }
    }

    public void ConnectAsObserver()
    {
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(Ident.Spectator + ',' + NewRoomField.text);
        }
        _slot.EmptyButtons();
    }
    public void SendMessageToChat()
    {
        if(textField.text.Length != 0)
        {
            SendMessageToHost(Ident.ChatMsg + ',' + textField.text);
            var newText = Instantiate(prefabText, GridForTexts.transform);
            if(turn)
            newText.GetComponent<TMP_Text>().text = textField.text;
            else
            newText.GetComponent<TMP_Text>().text = textField.text;
            textField.text = "";
        }
    }

   void ActivationForPlayer()
    {
        loserORwinner.gameObject.SetActive(true);
        textField.gameObject.SetActive(true);
        GridForTexts.gameObject.SetActive(true);
        stopWatching.gameObject.SetActive(false);
        chat.gameObject.SetActive(true);

    }
    public void DeactivateForSpectator()
    {
        canMove = false;
        turn = false;
        loserORwinner.gameObject.SetActive(false);
        textField.gameObject.SetActive(false);
        GridForTexts.gameObject.SetActive(false);
        stopWatching.gameObject.SetActive(true);
        chat.gameObject.SetActive(false);
    }
    public void StopWatching()
    {
        GameManager._instance.UpdateGameState(GameState.accountState);
        _slot.EmptyButtons();
        SendMessageToHost(Ident.stopWatching);
    }


    private void OnApplicationQuit()
    {
        if(login != null)
        {
            SendMessageToHost(Ident.exit);
            SendMessageToHost(Ident.dscnt + ',' + login);
        }
    }

}