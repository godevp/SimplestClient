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

    private const int turn = 1111;

    
    void Start()
    {
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SendResponseToHost(int number)
    {
        if(client.canMove)
        {
            client.SendMessageToHost(turn.ToString() + ',' + number.ToString());
        }
  
    }
}
