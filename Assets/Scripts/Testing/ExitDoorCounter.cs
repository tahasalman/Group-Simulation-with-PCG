using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoorCounter : MonoBehaviour {

    public float timerLength;
    public float numTravellers;

    private float count = 0;
    private Timer timer;
    
	// Use this for initialization
	void Start () {
        timer = new Timer(timerLength);
        timer.StartTimer();
	}
	
	// Update is called once per frame
	void Update () {
        timer.UpdateTimer();
        if (timer.timerOver() && !(timer.reset))
        {
            timer.ResetTimer();
            print(count / numTravellers);
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traveller"))
            count++;
    }
}
