using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public GameState _state;


    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        _instance = this;
    }

    public void UpdateGameState(GameState newState)
    {
        _state = newState;
        

        switch(newState)
        {
            case GameState.logingState:

                break;
            case GameState.accountState:
                NetworkedClientProcessing.Instance.LoginPart.SetActive(false);
                NetworkedClientProcessing.Instance.AccountPart.SetActive(true);
                NetworkedClientProcessing.Instance.GamePart.SetActive(false);
                break;
            case GameState.gameState:
                NetworkedClientProcessing.Instance.AccountPart.SetActive(false);
                NetworkedClientProcessing.Instance.GamePart.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnGameStateChanged?.Invoke(newState);
    }

}


public enum GameState
{
    logingState,
    accountState,
    gameState
}