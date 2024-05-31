using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class NPCYarnCommands : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    private void Start() {
        dialogueRunner = GetComponentInParent<DialogueRunner>();

        dialogueRunner.AddCommandHandler("npcSetFacing", NPCSetFacing);
        dialogueRunner.AddCommandHandler("npcMoveToX", NPCMoveToX);
    }

    private void NPCSetFacing(string[] parameters)
    {
        NPCController npc = GameObject.Find(parameters[0]).GetComponent<NPCController>();

        switch (parameters[1])
        {
            case "Left":
                npc.SetFacingDirection(true);
                break;
            case "Right":
                npc.SetFacingDirection(true);
                break;
        }
    }

    private void NPCMoveToX(string[] parameters)
    {
        NPCController npc = GameObject.Find(parameters[0]).GetComponent<NPCController>();
        float xPos = float.Parse(parameters[1]);
        bool leftFacingOnArrival = bool.Parse(parameters[2]);

        if (parameters.Length > 3)
        {
            float speed = float.Parse(parameters[3]);
            npc.MoveToXPosition(xPos, leftFacingOnArrival, speed);
        }
        else
        {
            npc.MoveToXPosition(xPos, leftFacingOnArrival);
        }
    }
}
