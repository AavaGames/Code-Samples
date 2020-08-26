using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private ActorManager actorManager;

    public enum ActorState { Idle, Independent, Group, Swarm, PlayerTarget, Exit }
    public ActorState state = ActorState.Idle;
    private float moveSpeed = 0;
    private bool explosiveMovement = false;

    public bool reachedPoI = true;
    private Transform pointOfInterest;
    private int prevPointOfInterestIndex = -1;

    private Transform swarmTarget;
    public Material baseMaterial;
    public Material swarmMaterial;
    public Material targetMaterial;
    public float maxMaterialChangeWaitTime = 1f;
    private bool swarmColor = false;
    private float velocityResistance = 0f;

    public int groupSeed = 0;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        actorManager = GetComponentInParent<ActorManager>();
    }

    void Update()
    {
        UpdateVariables();

        if (swarmColor && state != ActorState.Swarm)
        {
            swarmColor = false;
            StartCoroutine(DelayedSwitchToMaterial(baseMaterial));
        }

        if (state == ActorState.PlayerTarget)
        {
            gameObject.layer = LayerMask.NameToLayer("Target");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Actor");
        }

        switch (state)
        {
            case ActorState.Independent:
                if (reachedPoI)
                {
                    //Finds new Point of Interest
                    int index;
                    do
                    {
                        index = Random.Range(0, actorManager.pointsOfInterest.Length);
                    } while (index == prevPointOfInterestIndex);

                    pointOfInterest = actorManager.pointsOfInterest[index];

                    reachedPoI = false;
                }
                Move();
                break;

            case ActorState.Group:
                if (reachedPoI)
                {
                    Random.InitState(groupSeed);
                    //Finds new Point of Interest base on the groups seed
                    int index;
                    do
                    {
                        index = Random.Range(0, actorManager.pointsOfInterest.Length);
                    } while (index == prevPointOfInterestIndex);

                    pointOfInterest = actorManager.pointsOfInterest[index];

                    reachedPoI = false;
                }
                Move();
                break;

            case ActorState.Swarm:
                if (!swarmColor)
                {
                    swarmColor = true;
                    StartCoroutine(DelayedSwitchToMaterial(swarmMaterial));
                }
                Move();
                break;

            case ActorState.PlayerTarget:
                moveSpeed = moveSpeed * 2;
                PlayerMove();
                break;

            case ActorState.Exit:
                Move();
                break;

            default:
                break;
        }

        
        if (explosiveMovement)
        {
            //Just incase collisions cause them to get pushed
            Mathf.Clamp(rb.velocity.x, -moveSpeed * 2, moveSpeed * 2);
            Mathf.Clamp(rb.velocity.z, -moveSpeed * 2, moveSpeed * 2);
            rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, velocityResistance * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    private void UpdateVariables()
    {
        moveSpeed = actorManager.moveSpeed;
        explosiveMovement = actorManager.explosiveMovement;
        swarmTarget = actorManager.swarmTarget;
        velocityResistance = actorManager.velocityResistance;
    }

    private void Move()
    {
        Vector3 newPos;

        switch (state)
        {
            case ActorState.Independent:
                newPos = Vector3.MoveTowards(transform.position, pointOfInterest.position, moveSpeed * Time.deltaTime);
                break;

            case ActorState.Group:
                newPos = Vector3.MoveTowards(transform.position, pointOfInterest.position, moveSpeed * Time.deltaTime);
                break;

            case ActorState.Swarm:
                newPos = Vector3.MoveTowards(transform.position, swarmTarget.position, moveSpeed * Time.deltaTime);
                break;

            case ActorState.Exit:
                newPos = Vector3.MoveTowards(transform.position, Camera.main.transform.position, moveSpeed * Time.deltaTime);
                break;

            default:
                newPos = transform.position;
                break;
        }

        transform.LookAt(newPos);
        rb.MovePosition(newPos);
    }

    private void PlayerMove()
    {
        Vector3 direction;

        direction.x = -Input.GetAxis("Vertical");
        direction.y = 0f;
        direction.z = Input.GetAxis("Horizontal");

        //Corrects movement if movespeed is negative
        if (moveSpeed < 0)
        {
            direction *= -1f;
        }

        Vector3 newPos = transform.position + (direction * (moveSpeed * Time.deltaTime));
        rb.MovePosition(newPos);
    }

    private IEnumerator DelayedSwitchToMaterial(Material material)
    {
        float waitTime = Random.Range(0, maxMaterialChangeWaitTime);
        yield return new WaitForSeconds(waitTime);
        meshRenderer.material = material;
    }
}