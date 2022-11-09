using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    NetworkedClient client;
    const int winner = 777;
    [SerializeField] private List<bool> slotsTaken;
    [SerializeField] private List<Button> buttons;
    public Sprite Xsprite;
    public Sprite Osprite;
    private ColorBlock colorss;
    void Start()
    {
       
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SendResponseToHost(int number)
    {
        if (client.turn && !slotsTaken[number-1])//if first player send (X) to the certain slot
        {
            client.SendMessageToHost("X" + ',' + number.ToString());
            slotsTaken[number-1] = true;
            buttons[number - 1].GetComponent<Image>().sprite = Xsprite;

         
        }
           
        else if(!client.turn && !slotsTaken[number -1])//if second player send (O) to the certain slot
        {
            client.SendMessageToHost("O" + ',' + number.ToString());
            slotsTaken[number - 1] = true;
            buttons[number - 1].GetComponent<Image>().sprite = Osprite;
           
        }

        if ((slotsTaken[0] && slotsTaken[1] && slotsTaken[2]) ||
            (slotsTaken[3] && slotsTaken[4] && slotsTaken[5]) ||
            (slotsTaken[6] && slotsTaken[7] && slotsTaken[8]) ||
            (slotsTaken[0] && slotsTaken[3] && slotsTaken[6]) ||
            (slotsTaken[2] && slotsTaken[5] && slotsTaken[8]) ||
            (slotsTaken[2] && slotsTaken[4] && slotsTaken[6]) ||
            (slotsTaken[1] && slotsTaken[4] && slotsTaken[7]) ||
            (slotsTaken[0] && slotsTaken[4] && slotsTaken[8]))
        {
            client.SendMessageToHost(winner.ToString());
        }
    }

}
