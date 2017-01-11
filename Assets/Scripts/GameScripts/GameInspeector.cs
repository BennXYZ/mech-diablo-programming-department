﻿using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using XInputDotNetPure;

public class GameInspeector : MonoBehaviour
{
    public GameObject PlayerPrefab;
    private List<Player> spawnedPlayers;
	// Use this for initialization
	void Start ()
    {
        SpawnPlayers();
	}
	
	// Update is called once per frame
	void Update ()
    {
        CheckForNewPlayers();
	}

    private void SpawnPlayers()
    {
        spawnedPlayers = new List<Player>();

        for (int i = 0; i < GlobalReferences.PlayerStates.Count; i++)
        {
            SpawnPlayer(GlobalReferences.PlayerStates[i], new Vector3(i * 2, 1, 0));
        }
    }

    private void PlayerScript_OnPlayerExit(object sender, PlayerEventArgs e)
    {
        e.PlayerScript.OnPlayerExit -= PlayerScript_OnPlayerExit;
        spawnedPlayers.Remove(e.PlayerScript);
        RemovePlayerState(e.PlayerScript.Index);
        GamePadManager.Disconnect(e.PlayerScript.Index);
        
        Destroy(e.PlayerObject);
    }

    private void CheckForNewPlayers()
    {
        PlayerIndex[] freePads = GamePadManager.GetFreeControllers();
        for (int i = 0; i < freePads.Length; i++)
        {
            if(!IndexInUse(freePads[i]))
            {
                GamePadState state = GamePad.GetState(freePads[i]);
                if (state.IsConnected)
                {
                    if (state.Buttons.Start == ButtonState.Pressed)
                    {
                        PlayerState newPlayerState = new PlayerState(freePads[i], state, true, 0);
                        GlobalReferences.PlayerStates.Add(newPlayerState);
                        GamePadManager.Connect((int)freePads[i]);
                        SpawnPlayer(newPlayerState, new Vector3(0, 1, 0));
                    }
                }
            }
        }
    }

    private bool IndexInUse(PlayerIndex index)
    {
        for (int i = 0; i < spawnedPlayers.Count; i++)
        {
            if (spawnedPlayers[i].Index == index)
                return true;
        }

        return false;
    }

    private void SpawnPlayer(PlayerState playerState, Vector3 position)
    {
        GameObject newPlayer = Instantiate(PlayerPrefab, position, Quaternion.Euler(0, 0, 0));
        Player playerScript = newPlayer.GetComponent<Player>();
        playerScript.Index = playerState.Index;
        playerScript.OnPlayerExit += PlayerScript_OnPlayerExit;
        spawnedPlayers.Add(playerScript);
    }

    private void RemovePlayerState(PlayerIndex index)
    {
        for (int i = 0; i < GlobalReferences.PlayerStates.Count; i++)
        {
            if (GlobalReferences.PlayerStates[i].Index == index)
                GlobalReferences.PlayerStates.RemoveAt(i);
        }
    }
}