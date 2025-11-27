using UnityEngine;
using System.Collections.Generic;

public class CityGeneratorConnectedRandom : MonoBehaviour
{
    [Header("City Settings")]
    public int minTiles = 20;            // Nombre minimal de tiles
    public int maxTiles = 50;            // Nombre maximal de tiles
    public float tileSize = 0.96f;
    public GameObject cityTilePrefab;

    private int seed;
    private System.Random prng;

    private List<TileNode> placedTiles = new List<TileNode>();
    private List<TileNode> openEdges = new List<TileNode>();

    void Start()
    {
        seed = GlobalSeedManager.Instance.globalSeed;
        prng = new System.Random(seed);

        GenerateCity();
    }

    void GenerateCity()
    {
        // 1. Nombre de tiles aléatoire dans la range
        int numberOfTiles = prng.Next(minTiles, maxTiles + 1);
        Debug.Log($"[CityGenerator] Génération de {numberOfTiles} tiles avec seed globale {seed}");

        // 2. Placer la première tile au centre
        Vector3 startPos = Vector3.zero;
        GameObject firstTile = Instantiate(cityTilePrefab, startPos, Quaternion.identity, transform);
        firstTile.name = "Tile_0";

        TileNode firstNode = new TileNode(firstTile, Vector3.zero);
        placedTiles.Add(firstNode);
        openEdges.Add(firstNode);

        // 3. Placer les autres tiles
        for (int i = 1; i < numberOfTiles; i++)
        {
            // Choisir une tile avec bord ouvert
            int index = prng.Next(openEdges.Count);
            TileNode parent = openEdges[index];

            // Choisir un bord libre (N=+Z, E=+X, S=-Z, W=-X)
            Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            Vector3 dir = directions[prng.Next(directions.Length)];

            Vector3 newPos = parent.position + dir * tileSize;

            // Vérifier qu'il n'y a pas déjà une tile ici
            bool positionFree = true;
            foreach (var t in placedTiles)
                if (Vector3.Distance(t.position, newPos) < 0.1f)
                {
                    positionFree = false;
                    break;
                }

            if (!positionFree)
            {
                i--; // refaire ce tile
                continue;
            }

            // Instancier la tile
            GameObject tile = Instantiate(cityTilePrefab, newPos, Quaternion.identity, transform);
            tile.name = $"Tile_{i}";

            TileNode node = new TileNode(tile, newPos);
            placedTiles.Add(node);
            openEdges.Add(node);
        }
    }

    // Classe simple pour stocker les tiles placées
    private class TileNode
    {
        public GameObject tile;
        public Vector3 position;

        public TileNode(GameObject t, Vector3 pos)
        {
            tile = t;
            position = pos;
        }
    }
}
