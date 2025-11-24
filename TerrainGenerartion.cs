using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Mathematics.math;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public class TerrainGenerartion : MonoBehaviour
{

    [SerializeField] private Mesh mesh = null;
    [SerializeField] private List<Biomes> biomesList = new List<Biomes>();

    [SerializeField] private int seed = 3;
    [SerializeField] private float scale = 3f;
    [SerializeField] private float maxHeight = 3f;

    Vector3[] vertices = null;
    private List<Dictionary<BIOME, Color>> biomeList = new List<Dictionary<BIOME, Color>>();

    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        Random.InitState(seed);
        Color[] colors = new Color[vertices.Length];    

        Vector2 halfedMeshSize = new Vector2(mesh.bounds.size.x, mesh.bounds.size.z) / 2;
        for (int i = 0; i < vertices.Length; i++)
        {
            float heightMultiplicator =
                    (float)(Mathf.PerlinNoise(vertices[i].x * scale + halfedMeshSize.x, vertices[i].z * scale + halfedMeshSize.y)
                    + 0.5 * Mathf.PerlinNoise(vertices[i].x * 2 * scale + halfedMeshSize.x, vertices[i].z * 2 * scale + halfedMeshSize.y)
                    + 0.25 * Mathf.PerlinNoise(vertices[i].x * 4 * scale + halfedMeshSize.x, vertices[i].z * 4 * scale + halfedMeshSize.y));

            heightMultiplicator = heightMultiplicator / 1.75f;

            float height = maxHeight * heightMultiplicator;

            Vector3 verticeWorldPositon = transform.TransformPoint(vertices[i]);

            verticeWorldPositon.y += height;
            vertices[i].y = heightMultiplicator * maxHeight;

            //BIOME biome = GenerateBiomes(heightMultiplicator);

            //for (int b = 0; b < biomesList.Count; b++)
            //{
            //    if (biome == biomesList[b].biomeType)
            //    {
            //        colors[i] = biomesList[b].biomeColor;
            //        break;
            //    }
            //}

        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private BIOME GenerateBiomes(float heightMultiplicator)
    {
        for (int i = 0; i < biomesList.Count; i++)
        {
            if (heightMultiplicator < 0.1) return BIOME.OCEAN;
            if (heightMultiplicator < 0.12) return BIOME.BEACH;
        }

        if (heightMultiplicator > 0.8)
        {
            return BIOME.SNOW;
        }

        if (heightMultiplicator > 0.6)
        {
            return BIOME.TAIGA;
        }

        if (heightMultiplicator > 0.3)
        {
            return BIOME.GRASSLAND;
        }

        return BIOME.OCEAN;
    }

}
