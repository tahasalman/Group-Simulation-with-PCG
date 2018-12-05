using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SocialiteController : MonoBehaviour
{

    public float speed;
    public float force;
    public float disRadius;
    public float angleChange;
    public float socialRadius;
    public float coolDownTime;

    public Vector3 centerPoint;
    public Vector3 curVelocity;

    private Random rand;
    private float wanderAngle;
    private Rigidbody rb;
    private Timer wanderTimer;
    private Timer coolDownTimer;
    private Renderer[] renderers;
    // Use this for initialization
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rand = new Random();
        curVelocity = new Vector3(disRadius, 0, 0);

        wanderAngle = (float)(rand.NextDouble() * 360);
        rb = GetComponent<Rigidbody>();
        Vector3 centerPoint = transform.position;
        coolDownTimer = new Timer(coolDownTime);
        wanderTimer = new Timer(-1);
    }

    // Update is called once per frame
    void Update()
    {   if (coolDownTimer.timerOver()) {
            if (UpdateCenterPoint())
            {
                if (wanderTimer.reset)
                {
                    wanderTimer.StartTimer();
                }
                if (wanderTimer.timerOver())
                {
                    int action = rand.Next(0, 2);
                    if (action == 0)
                    {
                        if (Vector3.Distance(transform.position, centerPoint) > socialRadius)
                            Seek(centerPoint);
                    }
                    else
                        Wander();
                    wanderTimer.ResetTimer();
                    coolDownTimer.StartTimer();
                }
            }
            else
            {
                Wander();
            }
           }
        else
            Wander();
        wanderTimer.UpdateTimer();
        coolDownTimer.UpdateTimer();
    }

    bool UpdateCenterPoint()
    {
        int nearbySocials = 0;
        GameObject[] socialites = GameObject.FindGameObjectsWithTag("Socialite");
        Vector3 tempCent = transform.position;
        foreach (GameObject social in socialites)
        {
            float distance = Vector3.Distance(transform.position, social.transform.position);
            if (distance <= socialRadius && distance > 0)
            {
                tempCent += social.GetComponent<SocialiteController>().centerPoint;
                nearbySocials++;
            }
        }
        if (nearbySocials > 0)
        {
            tempCent /= (nearbySocials + 1);
            foreach (Renderer r in renderers)
                r.material.color = Color.yellow;
        }

        centerPoint = tempCent;
        if (nearbySocials > 0)
            return true;
        return false;
    }

    void Wander()
    {
        Vector3 circleCenter = new Vector3(curVelocity.x, curVelocity.y, curVelocity.z);
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


        wanderAngle += (float)rand.NextDouble() * angleChange - (angleChange * 0.5f);

        
        foreach (Renderer r in renderers)
            r.material.color = Color.green;
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
    {
        if (other.CompareTag("Exit") || other.CompareTag("Entrance"))
        {
            Vector3 tempVelocity = new Vector3(curVelocity.x * -3, 0, curVelocity.z * -3);
            transform.Translate(tempVelocity * Time.deltaTime);
        }
    }
}

public class Timer
{
    public bool reset = true;

    private float timer;
    private float timerLength;
    private Random rand;
    
    public Timer(float timerLength)
    {
        timer = 0;
        rand = new Random();
        this.timerLength = timerLength;
    }

    public void StartTimer()
    {
        if (timerLength > 0)
            timer = timerLength;
        else
            timer = (float)(rand.NextDouble() * 1.5 + 0.5);
        reset = false;
    }

    public void UpdateTimer()
    {
        timer -= Time.deltaTime;
    }

    public void ResetTimer()
    {
        timer = 0;
        reset = true;
    }

    public bool timerOver()
    {
        if (timer < 0)
            return true;
        else
            return false;
    }


}
