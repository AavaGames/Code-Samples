using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class PlayerYarnCommands : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    private void Start() {
        dialogueRunner = GetComponentInParent<DialogueRunner>();

        dialogueRunner.AddCommandHandler("playerSetState", SetPlayerState);
        dialogueRunner.AddCommandHandler("playerSetFacing", PlayerSetFacing);
        dialogueRunner.AddCommandHandler("playerMoveToX", PlayerMoveToX);
        dialogueRunner.AddCommandHandler("playerTetherToX", PlayerTetherToX);
        dialogueRunner.AddCommandHandler("playerUnTether", PlayerUnTether);
    }

    private PlayerPlatformerController FindPlayer()
    {
        return GameObject.FindWithTag("MasterPlayer").GetComponent<PlayerPlatformerController>();
    }

    private void SetPlayerState(string[] state)
    {
        PlayerPlatformerController player = FindPlayer();

        switch (state[0])
        {
            case "Cutscene":
                player.currentPlayerState = PlayerPlatformerController.PlayerState.Cutscene;
                break;
            case "CutsceneButCanJump":
                player.currentPlayerState = PlayerPlatformerController.PlayerState.CutsceneButCanJump;
                break;
            case "EnteringRoom":
                player.currentPlayerState = PlayerPlatformerController.PlayerState.EnteringRoom;
                break;
            default:
                player.currentPlayerState = PlayerPlatformerController.PlayerState.Control;
                break;
        }
    }

    private void PlayerSetFacing(string[] parameters)
    {
        PlayerPlatformerController player = FindPlayer();

        switch (parameters[0])
        {
            case "Left":
                player.CutsceneSetFacingDirection(true);
                break;
            case "Right":
                player.CutsceneSetFacingDirection(false);
                break;
        }
    }

    private void PlayerMoveToX(string[] parameters)
    {
        PlayerPlatformerController player = FindPlayer();

        float xPos = float.Parse(parameters[0]);
        bool leftFacingOnArrival = bool.Parse(parameters[1]);

        if (parameters.Length > 2)
        {
            float speed = float.Parse(parameters[2]);
            player.CutsceneMoveToXPosition(xPos, leftFacingOnArrival, speed);
        }
        else
        {
            player.CutsceneMoveToXPosition(xPos, leftFacingOnArrival);
        }
    }

    private void PlayerTetherToX(string[] parameters)
    {
        PlayerPlatformerController player = FindPlayer();

        float xPos = float.Parse(parameters[0]);
        float distance = float.Parse(parameters[1]);
        player.CutsceneTetherToX(xPos, distance);
    }

    private void PlayerUnTether(string[] parameters)
    {
        PlayerPlatformerController player = FindPlayer();

        player.CutsceneUnTether();
    }
}

