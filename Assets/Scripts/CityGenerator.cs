using UnityEngine;
using System.Collections.Generic;

public class CityClustersGeneratorTerrain : MonoBehaviour
{
    [Header("City Settings")]
    public GameObject cityTilePrefab;           // Prefab avec Road + Buildings
    public int minClusters = 2;
    public int maxClusters = 5;

    public int minTilesPerCluster = 5;
    public int maxTilesPerCluster = 15;

    [Tooltip("Taille approximative d’une tile (X/Z)")]
    public float tileSize = 2.5f;

    [Tooltip("Hauteur pour placer la tile au-dessus du terrain")]
    public float tileHeightOffset = 0.05f;

    [Tooltip("Distance minimale entre centres de clusters")]
    public float clusterSpacing = 20f;

    [Tooltip("Altitude minimum pour placer les clusters")]
    public float seaLevel = 0.25f;

    [Tooltip("Rotation aléatoire des tiles (0, 90, 180, 270)")]
    public bool randomTileRotation = true;

    private int seed;
    private System.Random prng;
    private Vector3[] terrainVertices;

    // Parent neutre pour organiser les tiles
    private GameObject cityParent;

    void Start()
    {
        // Seed globale
        seed = GlobalSeedManager.Instance.globalSeed;
        prng = new System.Random(seed);

        Mesh terrainMesh = GetComponent<MeshFilter>().mesh;
        terrainVertices = terrainMesh.vertices;

        // Création d’un parent neutre avec scale 1
        cityParent = new GameObject("CityTilesParent");

        SpawnCityClusters();
    }

    void SpawnCityClusters()
    {
        int numClusters = prng.Next(minClusters, maxClusters + 1);
        List<Vector3> clusterCenters = new List<Vector3>();

        for (int c = 0; c < numClusters; c++)
        {
            Vector3 clusterCenter = FindValidClusterCenter(clusterCenters);
            clusterCenters.Add(clusterCenter);

            int numTiles = prng.Next(minTilesPerCluster, maxTilesPerCluster + 1);
            GenerateCluster(clusterCenter, numTiles);
        }
    }

    Vector3 FindValidClusterCenter(List<Vector3> existingCenters)
    {
        int maxAttempts = 1000;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = transform.TransformPoint(
                terrainVertices[prng.Next(terrainVertices.Length)]
            );

            if (candidate.y < seaLevel) continue;

            bool tooClose = false;
            foreach (var center in existingCenters)
                if (Vector3.Distance(candidate, center) < clusterSpacing)
                {
                    tooClose = true;
                    break;
                }

            if (!tooClose) return candidate;
        }

        return transform.TransformPoint(terrainVertices[0]);
    }

    void GenerateCluster(Vector3 center, int numTiles)
    {
        List<Vector3> placedPositions = new List<Vector3>();
        List<Vector3> openEdges = new List<Vector3>();
        List<Quaternion> edgeRotations = new List<Quaternion>();

        //Première tile au centre
        GameObject firstTile = InstantiateTile(center, 0);
        placedPositions.Add(center);
        openEdges.Add(center);
        edgeRotations.Add(firstTile.transform.rotation);

        //Placer les autres tiles autour
        for (int i = 1; i < numTiles; i++)
        {
            int parentIndex = prng.Next(openEdges.Count);
            Vector3 parentPos = openEdges[parentIndex];
            Quaternion parentRot = edgeRotations[parentIndex];

            // Direction possible
            Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            Vector3 dir = directions[prng.Next(directions.Length)];

            Vector3 newPos = parentPos + parentRot * dir * tileSize;
            Quaternion newRot = parentRot;

            if (randomTileRotation)
            {
                int rotationStep = prng.Next(0, 4);
                newRot = Quaternion.Euler(0, rotationStep * 90f, 0) * newRot;
            }

            // Vérifier collision
            bool free = true;
            foreach (var pos in placedPositions)
                if (Vector3.Distance(pos, newPos) < 0.1f)
                {
                    free = false;
                    break;
                }

            if (!free)
            {
                i--;
                continue;
            }

            // Instancier la tile directement dans la scène
            GameObject tile = InstantiateTile(newPos, i, newRot);
            placedPositions.Add(newPos);
            openEdges.Add(newPos);
            edgeRotations.Add(newRot);
        }
    }

    GameObject InstantiateTile(Vector3 position, int index, Quaternion rotation = default)
    {
        GameObject tile = Instantiate(cityTilePrefab, position + Vector3.up * tileHeightOffset, rotation);
        tile.name = $"Tile_{index}_Cluster";

        // Mettre le parent neutre pour organiser
        tile.transform.parent = cityParent.transform;

        // Échelle neutre : pas affectée par le terrain
        tile.transform.localScale = Vector3.one * 1f;

        return tile;
    }
}
