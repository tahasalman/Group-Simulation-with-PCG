using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GameController : MonoBehaviour
{

    public int numObstacles = 2;
    public int numTravellers = 1;
    public int numWanderers = 1;
    public int numSocialites = 1;
    public bool canSpawn = true;

    public float travellerSpawnWaitTime = 0.5f;
    public GameObject obstacle;
    public GameObject traveller;
    public GameObject wanderer;
    public GameObject socialite;
    public float screenSize = 100;


    private Random rand = new Random();

    private int xMin = -80;
    private int xMax = 80;
    private int zMin = -40;
    private int zMax = 40;
    private WaitForSeconds wait;
    private WaitForSeconds spawnWait = new WaitForSeconds(0.1f);

    void Start()
    {
        canSpawn = true;
        wait = new WaitForSeconds(travellerSpawnWaitTime);

        //Generate Obstacles
        generateObstacles();
        //Generate Wanderers
        StartCoroutine(generateWanderers());

        //Generate Socialites
        StartCoroutine(generateSocialites());

        // Generate Travellers
        StartCoroutine(generateTravellers());
    }
    private void generateObstacles()
    {

        List<Vector3> coordsList = new List<Vector3>();
        float lowerB = 0.15f * screenSize;
        float upperB = 0.5f * screenSize;
        float screenToCover = (float)rand.NextDouble() * lowerB + (upperB - lowerB);
        float scale = (screenToCover/numObstacles)/3;
        for (int i = 0; i < numObstacles; i++)
        {
            int randX = rand.Next(xMin, (int)(xMax - scale*3));
            int randZ = rand.Next(zMin, zMax + 1);

            Vector3 coords = new Vector3(randX, 5, randZ);
            while (!(areCoordsOk(coordsList, coords, scale)))
            {
                randX = rand.Next(xMin, xMax + 1);
                randZ = rand.Next(zMin, zMax + 1);

                coords = new Vector3(randX, 5, randZ);

            }

            coordsList.Add(coords);

            int[,] data = new int[3, 3];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == 1 && y == 1)
                        data[x, y] = 1;
                    else
                    {
                        int num = rand.Next(0, 2);
                        data[x, y] = num;
                    }
                }
            }

            ObstacleMaker om = new ObstacleMaker(
                data,
                scale,
                coords,
                obstacle);
            om.make();
        }

    }

    private bool areCoordsOk(List<Vector3> oldCoords, Vector3 curCoord, float scale)
    {
        foreach(Vector3 coord in oldCoords)
        {
            if (Vector3.Distance(coord, curCoord) < scale * 3 || Math.Abs(coord.x - curCoord.x) < scale*3 || curCoord.x + scale*3 > xMax || curCoord.x - scale/2 <xMin)
                return false;
        }
        return true;
    }
    
    private IEnumerator generateTravellers()
    {
        for (int i = 0; i < numTravellers; i++)
        {
            GameObject.Instantiate(traveller);
            yield return wait;
        }
    }

    private IEnumerator generateWanderers()
    {
        for (int i = 0; i < numWanderers; i++)
        {
            int randX = rand.Next(xMin, xMax + 1);
            int randZ = rand.Next(zMin, zMax + 1);
            GameObject.Instantiate(wanderer, new Vector3(randX, 0, randZ), Quaternion.identity);
            yield return spawnWait;
        }
    }

    private IEnumerator generateSocialites()
    {
        for (int i = 0; i < numSocialites; i++)
        {
            int randX = rand.Next(xMin, xMax + 1);
            int randZ = rand.Next(zMin, zMax + 1);
            GameObject.Instantiate(socialite, new Vector3(randX, 0, randZ), Quaternion.identity);
            yield return spawnWait;
        }
    }

}
