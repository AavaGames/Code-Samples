using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class UniversalYarnCommands : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    public Canvas dialogueCanvas;
    private CinematicBars cinematicBars;
    private YarnSetSprite yarnSetSprite;


    private void Start()
    {
        dialogueRunner = GetComponentInParent<DialogueRunner>();
        cinematicBars = dialogueCanvas.GetComponent<CinematicBars>();
        yarnSetSprite = GetComponent<YarnSetSprite>();

        dialogueRunner.AddCommandHandler("debugLog", DebugLog);
        dialogueRunner.AddCommandHandler("setSprite", SetSprite);
        dialogueRunner.AddCommandHandler("setCharactersCurrentSprite", SetCharactersCurrentSprite);
        dialogueRunner.AddCommandHandler("showCinematicBars", ShowCinematicBars);
        dialogueRunner.AddCommandHandler("hideCinematicBars", HideCinematicBars);
    }

    private void DebugLog(string[] parameters)
    {
        Debug.Log(parameters);
    }

    private void SetSprite(string[] parameters)
    {
        string characterName = parameters[0];
        string spriteName = parameters[1];
        yarnSetSprite.SetSprite(characterName, spriteName);
    }

    private void SetCharactersCurrentSprite(string[] parameters)
    {
        string characterName = parameters[0];
        string spriteName = parameters[1];
        yarnSetSprite.SetCharactersCurrentSprite(characterName, spriteName);
    }

    private void ShowCinematicBars(string[] parameters)
    {
        float targetSize = float.Parse(parameters[0]);
        float time = float.Parse(parameters[1]);
        Debug.Log("Cine " + targetSize + " " + time);
        cinematicBars.ShowBars(targetSize, time);
    }

    private void HideCinematicBars(string[] parameters)
    {
        float time = float.Parse(parameters[0]);
        cinematicBars.HideBars(time);
    }
}
