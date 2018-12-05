using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WandererController : MonoBehaviour {

    public float speed;
    public float force;
    public float disRadius;
    public float angleChange;
    public float blockDistance;


    public Vector3 curVelocity;
    private Random rand;
    private float wanderAngle;
    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rand = new Random();
        curVelocity = new Vector3(disRadius, 0, 0);

        wanderAngle = (float)(rand.NextDouble() * 360);
        speed = (float)(rand.NextDouble() * speed + (speed/2));
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        if (! Interpose())
            Wander();
	}

    void Wander()
    {
        Vector3 circleCenter = new Vector3(curVelocity.x,curVelocity.y,curVelocity.z);
        circleCenter = Vector3.Normalize(circleCenter);
        circleCenter *= disRadius;

        Vector3 displacement = new Vector3(0, 0, -1);
        displacement *= disRadius;

        float disLength = Vector3.Magnitude(displacement);
        displacement.x = Mathf.Cos(wanderAngle) * disLength;
        displacement.z = Mathf.Sin(wanderAngle) * disLength;

        Vector3 steering = circleCenter + displacement;
        steering = Vector3.ClampMagnitude(steering, force);
        steering = steering / rb.mass;
        curVelocity = Vector3.ClampMagnitude(steering + curVelocity, speed);
        transform.Translate(curVelocity * Time.deltaTime);


        wanderAngle += (float)rand.NextDouble() * angleChange - (angleChange *0.5f);
    }

    bool Interpose()
    {
        GameObject[] travellers = GameObject.FindGameObjectsWithTag("Traveller");
        Vector3 currentPosition = transform.position;
        if (travellers.Length > 0)
        {
            float shortestTD = 1000;
            GameObject travellerSelected = travellers[0];
            foreach (GameObject t in travellers)
            {
                float tempDistance = Vector3.Distance(t.transform.position, currentPosition);
                if (tempDistance < shortestTD)
                {
                    shortestTD = tempDistance;
                    travellerSelected = t;
                }
            }

            if (shortestTD <= blockDistance)
            {
                Vector3 goal = travellerSelected.transform.position;
                goal.x -= 5;
                Seek(goal);
                return true;
            }
        }

        return false;
    }

    void Seek(Vector3 goal)
    {
        Vector3 position = transform.position - curVelocity;
        Vector3 desiredVelocity = Vector3.Normalize(goal - position) * speed;
        Vector3 steering = desiredVelocity - curVelocity;
        steering = Vector3.ClampMagnitude(steering, force);
        steering = steering / rb.mass;
        curVelocity = Vector3.ClampMagnitude(steering + curVelocity, speed);
        curVelocity.y = 0;
        transform.Translate(curVelocity * Time.deltaTime);

    }

    private void OnTriggerEnter(Collider other)
    {/*
        if (other.CompareTag("Exit") || other.CompareTag("Entrance"))
        {
            Vector3 tempVelocity = new Vector3(curVelocity.x * -3, 0, curVelocity.z * -3);
            transform.Translate(tempVelocity * Time.deltaTime);
        }
        */
    }
}
