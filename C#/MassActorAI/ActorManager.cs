using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorManager : MonoBehaviour
{
    [Header("Spawn Actors")]
    public int initialAmountOfActors = 10;
    public int addRemoveActors = 50;
    public GameObject actorPrefab;

    [Header("Arrays")]
    public List<Actor> actors;
    public Transform[] pointsOfInterest;

    [Header("Actor Variables")]
    //Used when new actors are made
    private Actor.ActorState state = Actor.ActorState.Independent;
    public bool explosiveMovement = false;
    public float moveSpeed = 20f;
    public float moveSpeedIncrement = 20f;
    public float velocityResistance = 5f;
    public Transform swarmTarget;

    [Header("Group AI Vars")]
    public int minGroups = 4;
    public int maxGroups = 10;
    List<int> seeds = new List<int>();

    private bool playing = true;

    void Start()
    {
        SpawnActors(initialAmountOfActors);
    }

    void Update()
    {
        if (playing)
        {
            InputToChangeActorStates();
        }
    }

    private void InputToChangeActorStates()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            state = Actor.ActorState.Idle;
            foreach (Actor actor in actors)
            {
                actor.state = Actor.ActorState.Idle;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            state = Actor.ActorState.Independent;
            foreach (Actor actor in actors)
            {
                actor.state = Actor.ActorState.Independent;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetGroupAI();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            state = Actor.ActorState.Swarm;
            foreach (Actor actor in actors)
            {
                actor.state = Actor.ActorState.Swarm;
            }
            //Choose a random actor to become the target
            Actor target = actors[Random.Range(0, actors.Count)];
            target.state = Actor.ActorState.PlayerTarget;
            swarmTarget = target.transform;
            target.GetComponent<MeshRenderer>().material = target.targetMaterial;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            explosiveMovement = !explosiveMovement;
        }
        else if (Input.GetKeyDown(KeyCode.Equals))
        {
            SpawnActors(addRemoveActors);
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            RemoveActors(addRemoveActors);
        }

        if (Input.GetKey(KeyCode.P))
        {
            moveSpeed += moveSpeedIncrement * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.O))
        {
            moveSpeed -= moveSpeedIncrement * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            playing = false;
            StartCoroutine(CalmlyExitTheApplication());
        }
    }

    void SetGroupAI()
    {
        seeds.Clear();

        state = Actor.ActorState.Group;
        foreach (Actor actor in actors)
        {
            actor.state = Actor.ActorState.Group;
        }

        int numOfGroups = Random.Range(minGroups, maxGroups + 1);

        //Too few actors adjusts group size
        if (numOfGroups > actors.Count)
        {
            numOfGroups = actors.Count;

            Debug.Log("Too few actors adjusting group size");
        }

        for (int i = 0; i < numOfGroups; i++)
        {
            //assign seeds
            seeds.Add(Random.Range(0, 1001));
        }

        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].groupSeed = seeds[Random.Range(0, seeds.Count)];
        }
    }

    void SpawnActors(int amount)
    {
        Debug.Log("Adding " + amount);

        GameObject tempObject;
        Vector3 center = Vector3.zero;
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = RandomCircle(center, 5f);
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center - pos);
            tempObject = Instantiate(actorPrefab, pos, rot);
            tempObject.name = "Actor";
            tempObject.transform.parent = gameObject.transform;
            tempObject.GetComponent<Actor>().state = state;
            if (state == Actor.ActorState.Group)
            {
                tempObject.GetComponent<Actor>().groupSeed = seeds[Random.Range(0, seeds.Count)];
            }
        }
        UpdateActorList();
    }

    void RemoveActors(int amount)
    {
        int index = actors.Count;
        for (int i = 0; i < amount; i++)
        {
            index--;

            //Minimum amount of actors is 2
            if (index <= 2)
            {
                break;
            }
            else if (actors[index].state == Actor.ActorState.PlayerTarget)
            {
                continue;
            }

            Destroy(actors[index].gameObject);
            actors.RemoveAt(index);
        }
    }

    void UpdateActorList()
    {
        actors.Clear();
        foreach (Actor child in GetComponentsInChildren<Actor>())
        {
            actors.Add(child);
        }
    }

    Vector3 RandomCircle(Vector3 center, float radius)
    {
        float ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }

    private IEnumerator CalmlyExitTheApplication()
    {
        Debug.Log("Quiting");

        state = Actor.ActorState.Exit;
        foreach (Actor actor in actors)
        {
            actor.state = Actor.ActorState.Exit;
            actor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            explosiveMovement = false;
            moveSpeed = 50;
        }

        yield return new WaitForSeconds(10.0f);
        Application.Quit();
    }
}