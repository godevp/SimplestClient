using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    NetworkedClientProcessing client;
    public List<Button> buttons;
    public Sprite Xsprite;
    public Sprite Osprite;
    public Sprite startSprite;


  

    void Start()
    {
        client = NetworkedClientProcessing.Instance;
    }

    public void SendResponseToHost(int number)
    {
        if(client.canMove)
        {
            client.SendMessageToHost(ClientToServerSignifiers.move.ToString() + ',' + number.ToString());
        }
    }

    public void ExitTheRoom()
    {
        client.SendMessageToHost(ClientToServerSignifiers.FeelTheListOfReplays.ToString());
        client.SendMessageToHost(ClientToServerSignifiers.exit.ToString());
        GameManager._instance.UpdateGameState(GameState.accountState);
        client.canMove = false;
        client.loserORwinner.text = "";
        EmptyButtons();
       
    }

    public void RestartTheRoom()
    {
        client.SendMessageToHost(ClientToServerSignifiers.restart.ToString());
    }

    public void EmptyButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponent<Image>().sprite = startSprite;
        }
    }
}
