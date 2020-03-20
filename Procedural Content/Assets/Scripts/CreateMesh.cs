using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Created by Will Pye



public class CreateMesh : MonoBehaviour {

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    public int xSize;
    public int zSize;
    Color[] colors;
    Vector2[] uv;
    public Gradient gradient;
    private float maxTerrainHeight = 0;
    private float minTerrainHeight = 100000;
    private int playerX;
    private int playerZ;
    private int lastPlayerX;
    private int lastPlayerZ;
    GameObject player;
    public float multiplier = 4;
    public float startFrequency = 1f;
    public float startAmplitude = 1f;
    public float startScale = 20f;
    public GameObject treeModel1;
    public GameObject treeModel2;
    public GameObject treeModel3;
    public GameObject shrubModel1;
    public GameObject shrubModel2;
    List<GameObject> shrubs = new List<GameObject>();
    List<Vector2> shrubCoords = new List<Vector2>();

    List<GameObject> trees = new List<GameObject>();
    List<Vector2> treeCoords = new List<Vector2>();


    //Variables used for generating Perlin Noise
    public int octaves = 4;
    public float persistance = .5f;
    public float lacunarity = 2;

    // This code uses a tutorial by Youtube Channel Brackeys
    //https://www.youtube.com/watch?v=64NblGkAabk

    // Use this for initialization
    void Start () {
        //Get Player Coordinates
        player = GameObject.Find("FPSController");
        GetPlayerCords();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //Create Triangles that form mesh
        CreateTriangles();
        //Update the Mesh with created triangles
        UpdateMesh();
        //Add the created mesh to the mesh collider to make walkable
        GetComponent<MeshCollider>().sharedMesh = mesh;

	}
	
    void GenerateShrubs(int x, int y, int z)
    {
        if (!shrubCoords.Contains(new Vector2(x, z)))
        {
            GameObject shrub;
            if (x % 3 == 0)
            {
                shrub = Instantiate(shrubModel1);
                shrub.transform.position = new Vector3(x, y, z);
            }
            else
            {
                shrub = Instantiate(shrubModel2);
                shrub.transform.position = new Vector3(x, y, z);
            }
            shrubCoords.Add(new Vector2(x, z));
            trees.Add(shrub);
        }
    }

    void RemoveShrubs(int maxX, int minX, int maxZ, int minZ)
    {
        List<GameObject> removeShrubs = new List<GameObject>();
        List<Vector2> removeShrubCoords = new List<Vector2>();
        foreach (GameObject shrub in shrubs)
        {
            float x = shrub.transform.position.x;
            float z = shrub.transform.position.z;
            if (x > maxX || x < minX || z > maxZ || z < minZ)
            {
                removeShrubCoords.Add(new Vector2((int)x, (int)z));
                removeShrubs.Add(shrub);
            }
        }
        foreach (Vector2 coords in removeShrubCoords)
        {
            shrubCoords.Remove(coords);
        }
        foreach (GameObject shrub in removeShrubs)
        {
            shrubs.Remove(shrub);
            Destroy(shrub);
        }
    }

    void GenerateTrees(int x, int y, int z)
    {
        if (!treeCoords.Contains(new Vector2(x, z))){
            GameObject tree;
            if(x % 8 == 0)
            {
                tree = Instantiate(treeModel1);
            }
            else if(y < 1) {
                tree = Instantiate(treeModel2);
            }
            else
            {
                tree = Instantiate(treeModel3);
            }
            tree.transform.position = new Vector3(x, y, z);
            treeCoords.Add(new Vector2(x, z));
            trees.Add(tree);
        }
    }

    void RemoveTrees(int maxX, int minX, int maxZ, int minZ)
    {
        List<GameObject> removeTrees = new List<GameObject>();
        List<Vector2> removeTreeCoords = new List<Vector2>();
        foreach(GameObject tree in trees)
        {
            float x = tree.transform.position.x;
            float z = tree.transform.position.z;
            if(x > maxX || x < minX || z > maxZ || z < minZ)
            {
                removeTreeCoords.Add(new Vector2((int)x, (int)z));
                removeTrees.Add(tree);
            }
        }
        foreach(Vector2 coords in removeTreeCoords)
        {
            treeCoords.Remove(coords);
        }
        foreach(GameObject tree in removeTrees)
        {
            trees.Remove(tree);
            Destroy(tree);
        }
    }

    void CreateTriangles()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[vertices.Length];
        uv = new Vector2[vertices.Length];
        int maxX = int.MinValue;
        int maxZ = int.MinValue;
        int minX = int.MaxValue;
        int minZ = int.MaxValue;


        int index = 0;
        //Create vertices
        //Create UV to display texture
        for (int z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                //Generate y value from perlin noise
                //Calculate x and z coordinates relative to player
                int relativeX = ((playerX + (xSize / 2)) - x);
                int relativeZ = ((playerZ + (zSize / 2)) - z);
                float y = GenerateNoise(relativeX,relativeZ);
                vertices[index] = new Vector3(relativeX, y, relativeZ);
                int yInt = (int)y;
                if (((yInt+4)*17+ relativeX*2 + relativeZ*3) % 100 == 0 && relativeX % 2 == 0 && relativeZ % 3 == 0)
                {
                    GenerateTrees(relativeX, yInt, relativeZ);
                }
                else if (((yInt + 6) * 13 + relativeX + relativeZ * 2) % 50 == 0 && relativeX % 4 == 0 && relativeZ % 3 == 0)
                {
                    GenerateShrubs(relativeX, yInt - 10, relativeZ);
                }
                uv[index] = new Vector2((float)relativeX / xSize, (float)relativeZ / zSize);
                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;
                maxX = (int)Mathf.Max(maxX,relativeX);
                maxZ = (int)Mathf.Max(maxZ, relativeZ);
                minX = (int)Mathf.Min(minX, relativeX);
                minZ = (int)Mathf.Min(minZ, relativeZ);
                index++;
            }
        }
        RemoveTrees(maxX, minX, maxZ, minZ);
        RemoveShrubs(maxX, minX, maxZ, minZ);
        //Create color based on height of vertice
        index = 0;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[index].y);
                colors[index] = gradient.Evaluate(height);
                index++;
            }
        }
        //Array to store the points of the triangles
        triangles = new int[xSize*zSize*6];

        //Track vertex number
        int vert = 0;
        //track triangle number
        int tri = 0;
        for(int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tri + 0] = vert + 0;
                triangles[tri + 1] = vert + xSize + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + xSize + 1;
                triangles[tri + 5] = vert + xSize + 2;
                vert++;
                //6 integers representing the corners of a triangle needed for each quad
                tri += 6;
            }
            //Stop script from wrapping a triangle around when moving up a row
            vert++;
        }

        

    }

    void GetPlayerCords()
    {
        lastPlayerX = playerX;
        lastPlayerZ = playerZ;
        playerX = (int)player.GetComponent<Transform>().position.x;
        playerZ = (int)player.GetComponent<Transform>().position.z;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        //Only update trianlges if player has moved
        if(lastPlayerX != playerX || lastPlayerZ != playerZ)
        {
            CreateTriangles();
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;
        mesh.colors = colors;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        
    }

	// Update is called once per frame
	void Update () {
        GetPlayerCords();
        UpdateMesh();
    }

    //Code below based off tutorial from youtube channel Sebastian Lague
    //https://www.youtube.com/watch?v=MRNFcywkUSA
    float GenerateNoise(int x, int z)
    {

        //float scale = startScale;
        //float amplitude = startAmplitude;
        //float frequency = startFrequency;
        //float y = 0f;
        //for(int i = 0; i < octaves; i++)
        //{
        //    float sampleX = (float)x /scale * frequency;
        //    float sampleZ = (float)z /scale * frequency;
        //    float perlinVal = (Mathf.PerlinNoise(sampleX, sampleZ)-1) * multiplier;
        //    y += perlinVal * amplitude;
        //    amplitude *= persistance;
        //    frequency *= lacunarity;
        //}
        //y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f + Mathf.PerlinNoise(x, z);
        float sampleX = (float)x / (float)xSize * startScale;
        float sampleZ = (float)z / (float)zSize * startScale;
        return Mathf.PerlinNoise(sampleX,sampleZ)*multiplier;
    }
}
