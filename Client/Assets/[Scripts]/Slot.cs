using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    NetworkedClient client;
    public List<Button> buttons;
    public Sprite Xsprite;
    public Sprite Osprite;
    public Sprite startSprite;


    
    void Start()
    {
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SendResponseToHost(int number)
    {
        if(client.canMove)
        {
            client.SendMessageToHost(Ident.move + ',' + number.ToString());
        }
    }


    public void ExitTheRoom()
    {
        client.SendMessageToHost(Ident.exit);
        GameManager._instance.UpdateGameState(GameState.accountState);
        client.canMove = false;
        client.loserORwinner.text = "";
        EmptyButtons();
    }

    public void RestartTheRoom()
    {
        client.SendMessageToHost(Ident.restart);
    }

    public void EmptyButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponent<Image>().sprite = startSprite;
        }
    }
}
