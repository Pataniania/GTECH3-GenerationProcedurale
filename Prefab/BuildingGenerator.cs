using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [System.Serializable]
    public class BuildingType
    {
        public GameObject prefab;
        public float weight;
    }

    public BuildingType[] buildingTypes;

    // Offsets des 4 coins
    public Vector3 offsetTopLeft = new Vector3(-0.5f, 0, 0.5f);
    public Vector3 offsetTopRight = new Vector3(0.5f, 0, 0.5f);
    public Vector3 offsetBottomLeft = new Vector3(-0.5f, 0, -0.5f);
    public Vector3 offsetBottomRight = new Vector3(0.5f, 0, -0.5f);

    private int seed;              // La seed locale
    private System.Random prng;     // Random stable par seed

    void Start()
    {
        //---------------------------------------
        // GÉNÉRATION SEED UNIQUE POUR CET OBJET
        //---------------------------------------
        int global = GlobalSeedManager.Instance.globalSeed;
        seed = CalculateUniqueSeed(global);

        prng = new System.Random(seed);

        Generate();
    }

    // Génère une seed locale unique (comme dans RoadGenerator)
    int CalculateUniqueSeed(int globalSeed)
    {
        unchecked
        {
            int id = gameObject.GetInstanceID();

            int hash = 17;
            hash = hash * 31 + globalSeed;
            hash = hash * 31 + id;

            return hash;
        }
    }

    void Generate()
    {
        SpawnBuilding(offsetTopLeft);
        SpawnBuilding(offsetTopRight);
        SpawnBuilding(offsetBottomLeft);
        SpawnBuilding(offsetBottomRight);
    }

    void SpawnBuilding(Vector3 offset)
    {
        GameObject prefab = GetRandomWeightedBuilding();

        // Rotations autorisées
        int[] rotations = { 0, 90, 180, -90 };
        int randomIndex = prng.Next(rotations.Length);
        int angle = rotations[randomIndex];

        Quaternion rot = Quaternion.Euler(0, angle, 0);

        Instantiate(prefab, transform.position + offset, rot, transform);
    }

    GameObject GetRandomWeightedBuilding()
    {
        float totalWeight = 0f;

        foreach (var b in buildingTypes)
            totalWeight += b.weight;

        // Random value selon total weight
        float value = (float)(prng.NextDouble() * totalWeight);

        foreach (var b in buildingTypes)
        {
            if (value < b.weight)
                return b.prefab;

            value -= b.weight;
        }

        return buildingTypes[buildingTypes.Length - 1].prefab;
    }
}
