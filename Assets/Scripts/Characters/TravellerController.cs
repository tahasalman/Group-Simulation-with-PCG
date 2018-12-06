using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class TravellerController : MonoBehaviour {

    public float averageSpeed = 1.0f;
    public float maxForce = 10;
    public float maxAvoidanceForce = 10;
    public float aheadLength = 10f;


    private Vector3 spawnPos;
    private Vector3 goal;
    private GameObject[] exits;
    private float tempSpeed;
    private float totalDistance;
    private Vector3 avoidanceForce = new Vector3(0, 0, 0);

    private Random rand;
    private Vector3 currentVelocity;
    private Rigidbody rb;
    private Vector3 ahead;
    private List<GameObject> collidingObjects = new List<GameObject>();

    private Timer positionTimer = new Timer(4);
    private Vector3 lastPos;
    private Timer goalTimer = new Timer(45);
	// Use this for initialization
	void Start () {

        rb = GetComponent<Rigidbody>();
        currentVelocity = new Vector3(averageSpeed, 0, 0);
        rand = new Random();
        spawnPos = GameObject.FindGameObjectWithTag("Entrance").transform.position;
        spawnPos.x -= 2;
        spawnPos.y = 0;

        GetComponent<BoxCollider>().size = new Vector3(aheadLength,1,1);
        GetComponent<BoxCollider>().center = new Vector3(-aheadLength / 2, 0, 0);


        exits = GameObject.FindGameObjectsWithTag("Exit");
        Respawn();
        totalDistance = Vector3.Distance(spawnPos, goal);

        positionTimer.StartTimer();
        goalTimer.StartTimer();
        lastPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        manageMovement();
        goalTimer.UpdateTimer();
        if (goalTimer.timerOver())
        {
            updateGoal();
            goalTimer.StartTimer();

        }
    }

    Vector3 getSeekForce()
    {
        Vector3 position = transform.position - currentVelocity;
        Vector3 desiredVelocity = Vector3.Normalize(goal - position) * tempSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }
    void Wander()
    {
        float disRadius = 5;
        float wanderAngle = wanderAngle = (float)(rand.NextDouble() * 360);
        Vector3 circleCenter = new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z);
        circleCenter = Vector3.Normalize(circleCenter);
        circleCenter *= disRadius;

        Vector3 displacement = new Vector3(0, 0, -1);
        displacement *= disRadius;

        float disLength = Vector3.Magnitude(displacement);
        displacement.x = Mathf.Cos(wanderAngle) * disLength;
        displacement.z = Mathf.Sin(wanderAngle) * disLength;

        Vector3 steering = circleCenter + displacement;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering = steering / rb.mass;
        currentVelocity = Vector3.ClampMagnitude(steering + currentVelocity, tempSpeed);
        transform.Translate(currentVelocity * Time.deltaTime);

    }


    void manageMovement()
    {
        if (positionTimer.timerOver())
        {
            if(Vector3.Distance(lastPos,transform.position) <= 5)
            {
                Wander();
                return;
            }
            positionTimer.StartTimer();
            lastPos = transform.position;
        }
        Vector3 steering = getSeekForce();
        //steering += getFleeForce();
        steering += getAvoidanceForce();
        steering = steering / rb.mass;
        currentVelocity = Vector3.ClampMagnitude(steering + currentVelocity, tempSpeed);
        currentVelocity.y = 0;
        transform.Translate(currentVelocity * Time.deltaTime);
        //avoidanceForce = new Vector3(0, 0, 0);

        positionTimer.UpdateTimer();

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit"))
            Respawn();

        if (other.CompareTag("Obstacle") || other.CompareTag("Wanderer") || other.CompareTag("Traveller"))
        {
            //updateAvoidanceForce(other);
            collidingObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Wanderer") || other.CompareTag("Traveller"))
        {
            collidingObjects.Remove(other.gameObject);
        }
    }

    private Vector3 getAvoidanceForce()
    {
        Vector3 steeringForce = new Vector3(0, 0, 0);
        if (collidingObjects.Count == 0)
            return steeringForce;
        else
        {
            GameObject closest = collidingObjects[0];
            float shortestDistance = Vector3.Distance(transform.position, closest.transform.position);
            for (int i = 0; i < collidingObjects.Count; i++)
            {
                float distance = Vector3.Distance(collidingObjects[i].transform.position, transform.position);
                if (distance < shortestDistance && distance != 0)
                {
                    closest = collidingObjects[i];
                    shortestDistance = distance;
                }
            }

            ahead = transform.position + Vector3.Normalize(currentVelocity) * aheadLength;

            steeringForce = GetComponent<BoxCollider>().bounds.ClosestPoint(ahead) - closest.transform.position;
            steeringForce.y = 0;
            Vector3.Normalize(steeringForce);
            steeringForce *= maxAvoidanceForce;
            return steeringForce;
        }
    }

    private void updateGoal()
    {
        int num = rand.Next(0, 2);
        goal = exits[num].transform.position;
    }

 
    public void Respawn()
    {
        while (!GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().canSpawn)
            continue;
        tempSpeed = (float)(rand.NextDouble()*averageSpeed + averageSpeed/2);
        updateGoal();
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().canSpawn = false;
        transform.position = spawnPos;
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().canSpawn = true;
    }
    
}
