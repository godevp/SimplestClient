using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplayButton : MonoBehaviour
{
    public TMP_Text text;



    public void WantThisReplay()
    {
        NetworkedClient.instace.SendMessageToHost(Ident.RequestForReplay + ',' + text.text);
        GameManager._instance.UpdateGameState(GameState.gameState);
        NetworkedClient.instace.DeactivateForSpectator();
        NetworkedClient.instace.stopWatching.gameObject.SetActive(false);
    }
}
