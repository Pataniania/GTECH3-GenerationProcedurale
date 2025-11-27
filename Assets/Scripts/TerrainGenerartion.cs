using System;
using UnityEngine;

[ExecuteAlways]
public class TerrainGenerartion : MonoBehaviour
{
    [Header("Forest")]
    [SerializeField] private GameObject[] treePrefabs = null;

    [Range(0, 150)][SerializeField] private int forestsSize = 30;
    [Range(0, 150)][SerializeField] private float patchRadius = 30f;
    [Range(0, 1)][SerializeField] private float forestThreshold = 0.5f;

    [Header("Terrain")]
    [SerializeField] private int seed = 3;

    [Range(0, 150)][SerializeField] private float scale = 6f;
    [SerializeField] private float maxHeight = 0.1f;
    [Range(0, 20)][SerializeField] private float terrainInclines = 1.7f;
    [SerializeField] private Mesh mesh = null;

    [SerializeField] private float seaLevel = 0.02f;

    [Range(0, 1)][SerializeField] private float maxHeightTreeSpawn = 0.46f;

    // Cached values to detect changes
    private int lastSeed = 0;
    private float lastScale = 0f;
    private float lastMaxHeight = 0f;
    private float lastTerrainInclines = 0f;

    private Vector3[] vertices = null;

    private void Start()
    {
        Initialize();
        GenerateTerrain();
    }

    private void OnValidate()
    {
        Initialize();

        // Only regenerate when important parameters change
        if (seed != lastSeed || scale != lastScale || maxHeight != lastMaxHeight)
        {
            GenerateTerrain();

            lastSeed = seed;
            lastScale = scale;
            lastMaxHeight = maxHeight;
            lastTerrainInclines = terrainInclines;
        }
    }

    private void Initialize()
    {
        // Needed so random choices remain consistent when using a seed
        UnityEngine.Random.InitState(seed);

        if (mesh == null)
            mesh = GetComponent<MeshFilter>().mesh;

        if (vertices == null || vertices.Length != mesh.vertices.Length)
        {
            vertices = mesh.vertices;

            lastSeed = seed;
            lastScale = scale;
            lastMaxHeight = maxHeight;
            lastTerrainInclines = terrainInclines;
        }
    }
    
    //Return a ridge-style noise (sharp mountains) based on Perlin noise.
    private float RidgeNoise(float nx, float ny)
    {
        return 2 * (0.5f - Mathf.Abs(0.5f - Mathf.PerlinNoise(nx, ny)));
    }

    // Apply noise-based heights to all terrain vertices.
    private void ApplyHeightToTerrain()
    {
        Vector2 halfedMeshSize = new Vector2(transform.lossyScale.x, transform.lossyScale.z) / 2;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Compute noise coordinates using mesh vertex position
            Vector2 octave = new Vector2(
                vertices[i].x * scale + halfedMeshSize.x,
                vertices[i].z * scale + halfedMeshSize.y
            );

            // Multiple layers of noise (octaves)
            float e0 = RidgeNoise(octave.x, octave.y);
            float e1 = 0.5f * RidgeNoise(2 * octave.x, 2 * octave.y) * e0;
            float e2 = 0.25f * RidgeNoise(4 * octave.x, 4 * octave.y) * (e0 + e1);

            float heightModifier = (e0 + e1 + e2) / 1.75f;

            // Apply slope control
            heightModifier = Mathf.Pow(heightModifier, terrainInclines);

            vertices[i].y = heightModifier * maxHeight;
        }
    }

    // Reassign vertices & update mesh collision and lighting.
    private void ApplyVerticesToMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Check if a tree can spawn at a given point based on height & density.
    private bool IsValidTreeLocation(Vector3 pos)
    {
        // Height constraints
        if (pos.y <= maxHeightTreeSpawn) return false;
        if (pos.y <= seaLevel) return false;

        // Density check (used to avoid clustering everywhere)
        float noiseValue = Mathf.PerlinNoise(pos.x * 0.1f, pos.z * 0.1f);
        float density = 1f - noiseValue;

        return density < forestThreshold;
    }


    // Generate all forest patches and their trees.
    private void GenerateForests()
    {
        for (int patch = 0; patch < forestsSize; patch++)
        {
            // Randomly pick a vertex to represent the forest patch center
            int patchCenterIndex = UnityEngine.Random.Range(0, vertices.Length);
            Vector3 patchCenter = transform.TransformPoint(vertices[patchCenterIndex]);

            float randomForestSize = UnityEngine.Random.Range(1, forestsSize);

            for (int t = 0; t < randomForestSize; t++)
            {
                TrySpawnTreeInPatch(patchCenter);
            }
        }
    }


    // Try to spawn a single tree inside a forest patch radius.
    // Includes attempts to find a valid location.

    private void TrySpawnTreeInPatch(Vector3 patchCenter)
    {
        int attempts = 0;
        int maxAttempts = vertices.Length * 2;

        int randomIndex = UnityEngine.Random.Range(0, vertices.Length);
        float randomRotation = UnityEngine.Random.Range(0f, 360f);

        while (attempts < maxAttempts)
        {
            Vector3 candidate = transform.TransformPoint(vertices[randomIndex]);

            // Only spawn inside the radius
            if (Vector3.Distance(candidate, patchCenter) <= patchRadius)
            {
                if (IsValidTreeLocation(candidate))
                {
                    int treeIndex = UnityEngine.Random.Range(0, treePrefabs.Length);
                    GameObject plant = Instantiate(treePrefabs[treeIndex], candidate, Quaternion.Euler(0, randomRotation, 0));
                    plant.transform.parent = transform;
                    break;
                }
            }

            randomIndex = UnityEngine.Random.Range(0, vertices.Length);
            attempts++;
        }
    }

    private void GenerateTerrain()
    {
        ApplyHeightToTerrain();
        ApplyVerticesToMesh();

        if (!Application.isPlaying) return;

        GenerateForests();
    }
}
