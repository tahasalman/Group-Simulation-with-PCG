using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public class VoxelData
{
    int[,] data;

    public VoxelData(int [,] data)
    {
        this.data = data;
    }

    public int Width
    {
        get { return data.GetLength(0); }
    }

    public int Depth
    {
        get { return data.GetLength(1); }
    }

    public int GetCell(int x, int z)
    {
        return data[x, z];
    }

}


public class VoxelRenderer : MonoBehaviour {

    public GameObject unit;
    public float scale;
    private Random rand = new Random();

	// Use this for initialization
	void Start () {
        int[,] data = new int[3, 3];
        for (int i=0; i<3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                int num = rand.Next(0, 2);
                data[i, j] = num;
                print(num);
            }
        }
        VoxelData vData = new VoxelData(data);
        for (int i=0; i<vData.Width; i++)
        {
            for (int j=0; j<vData.Depth; j++)
            {
                if (vData.GetCell(i,j) != 0)
                    GameObject.Instantiate(unit,new Vector3(transform.position.x + i*scale, 0, transform.position.z + j*scale),Quaternion.identity);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
