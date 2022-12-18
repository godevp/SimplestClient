using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Unity.VisualScripting;
#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int Spectator = 1;
    public const int ChatMsg = 2;
    public const int restart = 3;
    public const int stopWatching = 4;
    public const int exit = 5;
    public const int move = 6;
    public const int dscnt = 7;
    public const int room = 8;
    public const int reg = 9;
    public const int logIn = 10;
    public const int FeelTheListOfReplays = 11;
    public const int RequestForReplay = 12;
}


static public class ServerToClientSignifiers
{
    public const int LoginApproved = 1;
    public const int LoginDenied = 2;
    public const int Player = 3;
    public const int Spectator = 4;
    public const int ChatMsg = 5;
    public const int ObsUpdateX = 6;
    public const int ObsUpdateO = 7;
    public const int PlayerUpdateX = 8;
    public const int PlayerUpdateO = 9;
    public const int winner = 10;
    public const int loser = 11;
    public const int tie = 12;
    public const int turn1 = 13;
    public const int turn2 = 14;
    public const int restart = 15;
    public const int FeelTheListOfReplays = 16;
    public const int RequestForReplay = 17;
}

#endregion
public class NetworkedClientProcessing : MonoBehaviour
{
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
    private List<GameObject> listOfReplayObjects;

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
    #region instance of the class
    private static NetworkedClientProcessing instance;

    public static NetworkedClientProcessing Instance
    {
        get { return instance; }
    }
    #endregion
    #region Start() + SendMessageToHost()
    private void Start()
    {
        instance = this;
        ListOfReplays = new List<string>();
        listOfReplayObjects = new List<GameObject>();
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

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

                case "Login_Field":
                    logField = obj.GetComponent<TMP_InputField>();
                    break;
                case "Password_Field":
                    passwordField = obj.GetComponent<TMP_InputField>();
                    break;
                case "Chat":
                    chat = obj;
                    break;

                case "Obs_quit":
                    stopWatching = obj;
                        break;
                case "NewRoom_Field":
                    NewRoomField = obj.GetComponent<TMP_InputField>();
                    break;
                case "Input":
                    textField = obj.GetComponent<TMP_InputField>();
                    break;
                case "GridForReplay":
                    gridForReplay = obj;
                    break;
                case "GridForText":
                    GridForTexts = obj;
                    break;
                case "Error":
                    ErrorText = obj.GetComponent<TMP_Text>();
                    break;
                case "SlotPanel":
                    _slot = obj.GetComponent<Slot>();
                    break;
                case "loserORwinner":
                    loserORwinner = obj.GetComponent<TMP_Text>();
                    break;


                default:
                    break;
            }

        }
        GameManager._instance.UpdateGameState(GameState.logingState);
        logButton.GetComponent<Button>().onClick.AddListener(LogInF);
        registerButton.GetComponent<Button>().onClick.AddListener(RegisterF);
    }

    public void SendMessageToHost(string msg)
    {
        NetworkedClient.instace.SendMessageToHost(msg);
    }
    #endregion

    #region Receive message from Host
    public void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        string[] splitter = msg.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        switch (int.Parse(splitter[0]))
        {
            case ServerToClientSignifiers.LoginApproved:
                GameManager._instance.UpdateGameState(GameState.accountState);
                break;

            case ServerToClientSignifiers.LoginDenied:
                ErrorText.gameObject.SetActive(true);
                ErrorText.text = "Something wrong or somebody using the account";
                break;

            case ServerToClientSignifiers.Player:
                GameManager._instance.UpdateGameState(GameState.gameState);
                ActivationForPlayer();
                break;

            case ServerToClientSignifiers.Spectator:
                GameManager._instance.UpdateGameState(GameState.gameState);
                DeactivateForSpectator();
                break;

            case ServerToClientSignifiers.ChatMsg:

                var newText = Instantiate(prefabText, GridForTexts.transform);
                newText.GetComponent<TMP_Text>().text = splitter[1];
                break;

            case ServerToClientSignifiers.ObsUpdateX:
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Xsprite;

                break;
            case ServerToClientSignifiers.ObsUpdateO:
                _slot.buttons[int.Parse(splitter[1])].GetComponent<Image>().sprite = _slot.Osprite;
                break;

            case ServerToClientSignifiers.PlayerUpdateX:
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

            case ServerToClientSignifiers.PlayerUpdateO:
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

            case ServerToClientSignifiers.winner:  //win 
                canMove = false;
                loserORwinner.text = "Won";
                break;

            case ServerToClientSignifiers.loser://lose
                canMove = false;
                loserORwinner.text = "Lost";
                break;

            case ServerToClientSignifiers.tie://tie
                canMove = false;
                loserORwinner.text = "NO WINNER";
                break;

            case ServerToClientSignifiers.turn1:
                canMove = true;
                turn = true;//means we Xplayer
                loserORwinner.text = "Your Move";
                break;

            case ServerToClientSignifiers.turn2:
                canMove = false;
                turn = false;//means we Oplayer
                break;


            case ServerToClientSignifiers.restart:
                _slot.EmptyButtons();
                canMove = false;
                loserORwinner.text = "";
                break;

            case ServerToClientSignifiers.FeelTheListOfReplays:
                if (splitter[1] == "clean")
                {
                    foreach (GameObject x in listOfReplayObjects)
                    {
                        Destroy(x);
                    }
                    listOfReplayObjects.Clear();
                    ListOfReplays.Clear();
                }

                //foreach of these create a replay button and fill the who and where Moved.
                if (splitter[1] == "done")
                {
                    foreach (string _rep in ListOfReplays)
                    {
                        var newRep = Instantiate(prefabForReplay, gridForReplay.transform);
                        newRep.GetComponent<ReplayButton>().text.text = _rep;
                        listOfReplayObjects.Add(newRep);
                    }
                }
                if (splitter[1] != "done" && splitter[1] != "clean" && !ListOfReplays.Contains(splitter[1]))
                {
                    ListOfReplays.Add(splitter[1]);
                }
                break;
            case ServerToClientSignifiers.RequestForReplay:
                if (splitter[1] == "1")
                {
                    _slot.buttons[int.Parse(splitter[2])].GetComponent<Image>().sprite = _slot.Xsprite;
                }
                if (splitter[1] == "2")
                {
                    _slot.buttons[int.Parse(splitter[2])].GetComponent<Image>().sprite = _slot.Osprite;
                }
                if (splitter[1] == "obsExit")
                {
                    stopWatching.gameObject.SetActive(true);
                }


                break;



            default:
                break;
        }
    }
    #endregion
    #region support functions
    public void LogInF()
    {
        if (logField.text.Length != 0)
            login = logField.text;
        if (passwordField.text.Length != 0)
            password = passwordField.text;

        if (logField.text.Length != 0 && passwordField.text.Length != 0)
        {
            SendMessageToHost(ClientToServerSignifiers.logIn.ToString() + ',' + login + ',' +
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
            SendMessageToHost(ClientToServerSignifiers.reg.ToString() + ',' + login + ',' +
                              password);
        }
    }
    public void CreateRoom()
    {
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(ClientToServerSignifiers.room.ToString() + ',' + NewRoomField.text);
        }
    }

    public void ConnectAsObserver()
    {
        if (NewRoomField.text.Length != 0)
        {
            SendMessageToHost(ClientToServerSignifiers.Spectator.ToString() + ',' + NewRoomField.text);
        }
        _slot.EmptyButtons();
    }
    public void SendMessageToChat()
    {
        if (textField.text.Length != 0)
        {
            SendMessageToHost(ClientToServerSignifiers.ChatMsg.ToString() + ',' + textField.text);
            var newText = Instantiate(prefabText, GridForTexts.transform);
            if (turn)
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
        SendMessageToHost(ClientToServerSignifiers.stopWatching.ToString());
        SendMessageToHost(ClientToServerSignifiers.FeelTheListOfReplays.ToString());
    }


    private void OnApplicationQuit()
    {
        if (login != null)
        {
            SendMessageToHost(ClientToServerSignifiers.exit.ToString());
            SendMessageToHost(ClientToServerSignifiers.dscnt.ToString() + ',' + login);
        }
    }
    #endregion
}
