using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMaker {

    private int[,] data;
    private float scale;
    private Vector3 startingPosition;
    private GameObject unit;
    private float unitLength;

    public ObstacleMaker(int [,] data, float scale, Vector3 startingPosition, GameObject unit)
    {
        this.data = data;
        this.scale = scale;
        this.startingPosition = startingPosition;
        this.unit = unit;
        unitLength = unit.transform.localScale.x;
    }

    public void make()
    {
        for (int i=0; i< data.GetLength(0); i++)
        {
            for(int j=0; j<data.GetLength(1); j++)
            {
                if (data[i,j] != 0)
                {
                    GameObject g = GameObject.Instantiate(unit, new Vector3(startingPosition.x + i *scale,startingPosition.y, startingPosition.z + j * scale), Quaternion.identity);
                    g.transform.localScale = new Vector3(scale, unitLength, scale);
                }
            }
        }
    }
}
