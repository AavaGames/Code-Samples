using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelColorComponent : MonoBehaviour
{
    public enum LevelComponentType { DangerZone, MoveZone, StopZone, BeatLineBonus, BeatLineSide, HealthBar }
    public LevelComponentType componentType;
    private LevelColorEditor levelColorEditor;
    private SpriteRenderer spriteRenderer;

    private void Start() {
        levelColorEditor = GameObject.FindWithTag("LevelManager").GetComponent<LevelColorEditor>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        switch(componentType)
        {
            case LevelComponentType.DangerZone:
                spriteRenderer.color = levelColorEditor.dangerZone;
                break;
            case LevelComponentType.MoveZone:
                spriteRenderer.color = levelColorEditor.moveZone;
                break;
            case LevelComponentType.StopZone:
                spriteRenderer.color = levelColorEditor.stopZone;
                break;
            case LevelComponentType.BeatLineBonus:
                spriteRenderer.color = levelColorEditor.beatLineBonus;
                break;
            case LevelComponentType.BeatLineSide:
                spriteRenderer.color = levelColorEditor.beatLineSide;
                break;
            case LevelComponentType.HealthBar:
                spriteRenderer.color = levelColorEditor.healthBar;
                break;
            default:
                break;
        }
    }
}
