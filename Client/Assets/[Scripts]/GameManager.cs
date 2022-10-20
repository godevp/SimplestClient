using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public GameState _state;

    private NetworkedClient _client;

    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        _instance = this;
    }

    public void Start()
    {
        UpdateGameState(GameState.logingState);
        _client = FindObjectOfType<NetworkedClient>();
    }

    public void UpdateGameState(GameState newState)
    {
        _state = newState;
        

        switch(newState)
        {
            case GameState.logingState:
                Debug.Log("we in login state");

                break;
            case GameState.accountState:
                _client.LoginPart.SetActive(false);
                _client.AccountPart.SetActive(true);

                Debug.Log("we in account state");
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
    accountState
}