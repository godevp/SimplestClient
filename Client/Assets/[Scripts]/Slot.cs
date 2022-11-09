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
    [SerializeField] private List<int> slotsByPlayer;
    [SerializeField] private List<Button> buttons;
    public Sprite Xsprite;
    public Sprite Osprite;

    private const int turn = 1111;

    
    void Start()
    {
        client = FindObjectOfType<NetworkedClient>();
    }

    public void SendResponseToHost(int number)
    {
        if(!client.gameOver && client.canMove)
        {
            if (client.turn && !slotsTaken[number - 1] && client.connectionNumber < 3)//if first player send (X) to the certain slot
            {
                client.SendMessageToHost(turn.ToString() + ',' + number.ToString());
                slotsTaken[number - 1] = true;
                slotsByPlayer[number - 1] = 1;
                buttons[number - 1].GetComponent<Image>().sprite = Xsprite;
                client.canMove = false;

            }

            else if (!client.turn && !slotsTaken[number - 1] && client.connectionNumber < 3)//if second player send (O) to the certain slot
            {
                client.SendMessageToHost(turn.ToString() + ',' + number.ToString());
                slotsTaken[number - 1] = true;
                slotsByPlayer[number - 1] = 1;
                buttons[number - 1].GetComponent<Image>().sprite = Osprite;
                client.canMove = false;
            }

            //if ((slotsTaken[0] && slotsTaken[1] && slotsTaken[2]) ||
            //    (slotsTaken[3] && slotsTaken[4] && slotsTaken[5]) ||
            //    (slotsTaken[6] && slotsTaken[7] && slotsTaken[8]) ||
            //    (slotsTaken[0] && slotsTaken[3] && slotsTaken[6]) ||
            //    (slotsTaken[2] && slotsTaken[5] && slotsTaken[8]) ||
            //    (slotsTaken[2] && slotsTaken[4] && slotsTaken[6]) ||
            //    (slotsTaken[1] && slotsTaken[4] && slotsTaken[7]) ||
            //    (slotsTaken[0] && slotsTaken[4] && slotsTaken[8]))
            //{
                if((slotsByPlayer[0] == 1 && slotsByPlayer[1] == 1 && slotsByPlayer[2] == 1) ||
                   (slotsByPlayer[3] == 1 && slotsByPlayer[4] == 1 && slotsByPlayer[5] == 1) ||
                   (slotsByPlayer[6] == 1 && slotsByPlayer[7] == 1 && slotsByPlayer[8] == 1) ||
                   (slotsByPlayer[0] == 1 && slotsByPlayer[3] == 1 && slotsByPlayer[6] == 1) ||
                   (slotsByPlayer[2] == 1 && slotsByPlayer[5] == 1 && slotsByPlayer[4] == 1) ||
                   (slotsByPlayer[2] == 1 && slotsByPlayer[4] == 1 && slotsByPlayer[6] == 1) ||
                   (slotsByPlayer[1] == 1 && slotsByPlayer[4] == 1 && slotsByPlayer[7] == 1) ||
                   (slotsByPlayer[0] == 1 && slotsByPlayer[4] == 1 && slotsByPlayer[8] == 1))
                { 
                client.SendMessageToHost(winner.ToString());
                client.loserORwinner.text = "Won";
                client.gameOver = true;
            }
        }
  
    }

    public void SetTheSlotToTaken(int slotNumber)
    {
        slotsTaken[slotNumber-1] = true;
        if (client.turn)
        {
            buttons[slotNumber-1].GetComponent<Image>().sprite = Osprite;
        }
        else
        {
            buttons[slotNumber-1].GetComponent<Image>().sprite = Xsprite;
        }
    }

}
