using UnityEngine;
using System.Collections.Generic;

public class CityGeneratorConnectedRandom : MonoBehaviour
{
    [Header("City Settings")]
    public GameObject cityTilePrefab;     // Prefab contenant Road + Building
    public int minCityTiles = 10;
    public int maxCityTiles = 30;
    public float tileSize = 0.96f;
    public float maxSlope = 0.3f;        // pente maximale pour placer la ville
    public float seaLevel = 0.25f;

    private TerrainGenerartion terrainScript;

    void Start()
    {
        terrainScript = GetComponent<TerrainGenerartion>();

        if (terrainScript == null)
        {
            Debug.LogError("TerrainGenerartion script not found on this GameObject!");
            return;
        }

        SpawnCityOnTerrain();
    }

    void SpawnCityOnTerrain()
    {
        // Seed globale
        int seed = GlobalSeedManager.Instance.globalSeed;
        System.Random prng = new System.Random(seed);

        // Nombre de tiles aléatoire
        int numberOfTiles = prng.Next(minCityTiles, maxCityTiles + 1);

        Vector3[] vertices = terrainScript.GetComponent<MeshFilter>().mesh.vertices;

        List<Vector3> validPositions = new List<Vector3>();

        // Trouver les positions valides (au-dessus du seaLevel et pente faible)
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            if (worldPos.y > seaLevel)
            {
                validPositions.Add(worldPos);
            }
        }

        if (validPositions.Count == 0)
        {
            Debug.LogWarning("No valid positions for city tiles on terrain.");
            return;
        }

        // Placer les tiles
        for (int i = 0; i < numberOfTiles; i++)
        {
            int index = prng.Next(validPositions.Count);
            Vector3 position = validPositions[index];

            // On retire cette position pour éviter de superposer
            validPositions.RemoveAt(index);

            GameObject tile = Instantiate(cityTilePrefab, position, Quaternion.identity, transform);
            tile.name = $"CityTile_{i}";
        }
    }
}
