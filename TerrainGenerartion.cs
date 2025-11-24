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
    [SerializeField] private List<GameObject> landscapeList = new List<GameObject>();

    [SerializeField] private int seed = 3;
    [SerializeField] private float scale = 3f;
    [SerializeField] private float maxHeight = 3f;
    [SerializeField] private float landscapeQuantity = 10;

    Vector3[] vertices = null;
    private List<Dictionary<BIOME, Color>> biomeList = new List<Dictionary<BIOME, Color>>();

    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        Debug.Log(transform.lossyScale);

        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        Random.InitState(seed);
        Color[] colors = new Color[vertices.Length];    

        Vector2 halfedMeshSize = new Vector2(transform.lossyScale.x,transform.lossyScale.z) / 2;
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

            Debug.Log(verticeWorldPositon);

        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        for (int j = 0; j < landscapeQuantity; j++)
        {
            int landscapeIndex = Random.Range(0, landscapeList.Count);
            int randomVerticeRange = Random.Range(0, vertices.Length);

            Vector3 landscapeRandomPosition = transform.TransformPoint(vertices[randomVerticeRange]) ;

            Instantiate(landscapeList[landscapeIndex], landscapeRandomPosition, Quaternion.identity);
        }
    }

}
