using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectZone : MonoBehaviour
{
    PlayerController controller;

    public float maxdmgTimer = 0.1f;
    private float dmgTimer = 0f;
    public float damage = 1f;
#if UNITY_STANDALONE
    private float stopTimer = 0f;
#endif
    public static bool isInZone = false;

    private Camera MainCamera;
    private float decPass = 7500f;
    private float decPassSpeed = 1000f;

    private float regenTimer = 0f;
    private float maxRegenTimer = 2f;

    public float regenAmount = 0.1f;

    private float timer = 0f;
    private float maxTimer = 0.1f;


    private void Start()
    {
        MainCamera = Camera.main;
        MainCamera.GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;

        controller = gameObject.GetComponent<PlayerController>();
    }

    private void Update() {
        LowPassDamageIndicator();
        Regeneration();
    }

    private void LowPassDamageIndicator()
    {
        if (isInZone)
        {
            if (decPass == 22000) decPass = 7500;
            MainCamera.GetComponent<AudioLowPassFilter>().cutoffFrequency = decPass;
            decPass -= decPassSpeed;
            decPass = Mathf.Clamp(decPass, 500f, 7500f);
        }
        else if (!isInZone)
        {
            if (decPass == 7500) decPass = 22000;
            MainCamera.GetComponent<AudioLowPassFilter>().cutoffFrequency = decPass;
            decPass += decPassSpeed;
            decPass = Mathf.Clamp(decPass, 500f, 7500f);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("DangerZone"))
        {
            DamageOverTime();
        }
        else if (other.CompareTag("MoveZone") && !controller.currentlyMoving)
        {
            DamageOverTime();
        }
#if UNITY_STANDALONE
        else if (other.CompareTag("StopZone") && controller.currentlyMoving)
        {
            StopTimer();
        }
        else if (other.CompareTag("StopZone") && !controller.currentlyMoving)
        {
            stopTimer = 0f;
        }
#endif
#if (UNITY_ANDROID || UNITY_IOS)
        else if (other.CompareTag("StopZone") && controller.currentlyMoving)
        {
            DamageOverTime();
        }
#endif
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isInZone = false;
    }

    void DamageOverTime()
    {
        isInZone = true;

        regenTimer = maxRegenTimer;

        dmgTimer += Time.deltaTime;
        if (dmgTimer >= maxdmgTimer)
        {
            dmgTimer = 0;
            PlayerStats.TakingDamage(damage);
        }
    }
#if UNITY_STANDALONE
    void StopTimer()
    {
        stopTimer += Time.deltaTime;
        if (stopTimer >= GetComponent<PlayerController>().currentlyMovingFalloffBuffer)
        {
            DamageOverTime();
        }
    }
#endif

    private void Regeneration()
    {
        if (regenTimer > 0)
        {
            regenTimer -= Time.deltaTime;
        }

        if (regenTimer <= 0)
        {
            timer += Time.deltaTime;
            if (timer >= maxTimer)
            {
                timer = 0;
                PlayerStats.AddHealth(regenAmount);
            }
        }
    }
}

