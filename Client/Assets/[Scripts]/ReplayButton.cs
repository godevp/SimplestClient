using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReplayButton : MonoBehaviour
{
    public TMP_Text text;
    public void WantThisReplay()
    {
        NetworkedClientProcessing.Instance.SendMessageToHost(ClientToServerSignifiers.RequestForReplay.ToString() + ',' + text.text);
        GameManager._instance.UpdateGameState(GameState.gameState);
        NetworkedClientProcessing.Instance.DeactivateForSpectator();
        NetworkedClientProcessing.Instance.stopWatching.gameObject.SetActive(false);
    }
}
