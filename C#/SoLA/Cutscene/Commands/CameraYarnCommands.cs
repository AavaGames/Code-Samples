using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Cinemachine;

public class CameraYarnCommands : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    private CameraController cameraController;
    private void Start()
    {
        dialogueRunner = GetComponentInParent<DialogueRunner>();
        cameraController = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();

        dialogueRunner.AddCommandHandler("cameraStart", StartCutscene);
        dialogueRunner.AddCommandHandler("cameraEnd", EndCutscene);
        dialogueRunner.AddCommandHandler("changeCamera", ChangeCamera);
    }

    private void StartCutscene(string[] parameters)
    {
        cameraController.StartCutscene();
    }

    private void EndCutscene(string[] parameters)
    {
        cameraController.EndCutscene();
    }

    private void ChangeCamera(string[] parameters)
    {
        cameraController.ChangeCamera(parameters[0]);
    }
}
