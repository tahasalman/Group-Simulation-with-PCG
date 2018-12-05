using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class TravellerController : MonoBehaviour {

    public float averageSpeed = 1.0f;
    public float maxForce = 10;
    public float maxAvoidanceForce = 10;
    public float fleeRadius = 15;
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
        lastPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        manageMovement();
    }


    void Seek()
    {
        Vector3 position = transform.position - currentVelocity;
        Vector3 desiredVelocity = Vector3.Normalize(goal - position) * tempSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering = steering/rb.mass;
        currentVelocity = Vector3.ClampMagnitude(steering + currentVelocity, tempSpeed);
        transform.Translate(currentVelocity*Time.deltaTime);

    }

    void Flee()
    {
        Vector3 position = transform.position - currentVelocity;
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Vector3 steering = new Vector3(0, 0, 0);

        foreach(GameObject obstacle in obstacles){
            if (Vector3.Distance(obstacle.transform.position, transform.position) <= fleeRadius)
            {
                Vector3 desiredVelocity = Vector3.Normalize(position - obstacle.transform.position) * tempSpeed;
                steering += (desiredVelocity - currentVelocity);
            }
        }
        steering = steering / rb.mass;
        currentVelocity = Vector3.ClampMagnitude(steering + currentVelocity, tempSpeed);
        transform.Translate(currentVelocity * Time.deltaTime);
        print(currentVelocity * Time.deltaTime);
    }

    Vector3 getSeekForce()
    {
        Vector3 position = transform.position - currentVelocity;
        Vector3 desiredVelocity = Vector3.Normalize(goal - position) * tempSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        float dist = Vector3.Distance(goal, position);
        //steering *= (totalDistance - dist + 20 / totalDistance);
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    Vector3 getFleeForce()
    {
        Vector3 position = transform.position - currentVelocity;
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Vector3 steering = new Vector3(0, 0, 0);

        foreach(GameObject obstacle in obstacles){
            float distance = Vector3.Distance(obstacle.transform.position, transform.position);
            if (distance <= fleeRadius)
            {
                Vector3 desiredVelocity = Vector3.Normalize(position - obstacle.transform.position) * tempSpeed;
                Vector3 tempSteering = (desiredVelocity - currentVelocity);
                //tempSteering *= (fleeRadius - distance + 1 / fleeRadius);
                //if (distance <= 11)
                //    tempSteering *= 2;
                //tempSteering /= ((fleeRadius - distance + 1)/fleeRadius - 10);
                steering += tempSteering;
            }
        }
        return Vector3.ClampMagnitude(steering,maxForce);
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
                print("Wander MF");
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

    private void updateAvoidanceForce(Collider other)
    {
        ahead = transform.position + Vector3.Normalize(currentVelocity) * aheadLength;
        print(ahead);
        avoidanceForce = GetComponent<BoxCollider>().bounds.ClosestPoint(ahead) - other.transform.position;
        //avoidanceForce.x = currentVelocity.x - aheadVector - other.transform.position.x;
        //avoidanceForce.y = currentVelocity.z - aheadVector - other.transform.position.z;
        avoidanceForce.y = 0;
        Vector3.Normalize(avoidanceForce);
        avoidanceForce *= maxAvoidanceForce;
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
        
        
        /*
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        GameObject[] wanderers = GameObject.FindGameObjectsWithTag("Wanderer");
        GameObject[] travellers = GameObject.FindGameObjectsWithTag("Traveller");

        List<GameObject> collidingObjects = new List<GameObject>();
        
        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
                collidingObjects.Add(obstacle);
        }

        foreach (GameObject wanderer in wanderers)
        {
            if (wanderer.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
                collidingObjects.Add(wanderer);
        }
        foreach (GameObject traveller in travellers)
        {
            if (traveller.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
                collidingObjects.Add(traveller);
        }

        if (collidingObjects.Count == 0)
            return new Vector3(0, 0, 0);


        GameObject closest = collidingObjects[0];
        float shortestDistance = Vector3.Distance(transform.position, closest.transform.position);
        for(int i=0; i<collidingObjects.Count; i++)
        {
            float distance = Vector3.Distance(collidingObjects[i].transform.position, transform.position);
            if (distance < shortestDistance && distance != 0) 
            {
                closest = collidingObjects[i];
                shortestDistance = distance; 
            }
        }

        if (shortestDistance > aheadLength)
            return new Vector3(0, 0, 0);

        ahead = transform.position + Vector3.Normalize(currentVelocity) * aheadLength;
        
        Vector3 steeringForce = GetComponent<BoxCollider>().bounds.ClosestPoint(ahead) - closest.transform.position;
        steeringForce.y = 0;
        Vector3.Normalize(steeringForce);
        steeringForce *= maxAvoidanceForce;
        print(steeringForce);
        return steeringForce;
        */
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
