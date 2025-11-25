using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{

    [System.Serializable]
    public class BuildingType
    {
        public GameObject prefab; // le prefab du bâtiment
        public float weight;      // le poids du bâtiment
    }

    public BuildingType[] buildingTypes;

    // Offsets des 4 coins de la tuile
    public Vector3 offsetTopLeft = new Vector3(-0.5f, 0, 0.5f);
    public Vector3 offsetTopRight = new Vector3(0.5f, 0, 0.5f);
    public Vector3 offsetBottomLeft = new Vector3(-0.5f, 0, -0.5f);
    public Vector3 offsetBottomRight = new Vector3(0.5f, 0, -0.5f);

    void Start()
    {
        Generate();
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

        // Choisir une rotation parmi 0, 90, 180, -90
        int[] rotations = { 0, 90, 180, -90 };
        int randomRot = rotations[Random.Range(0, rotations.Length)];

        Quaternion rot = Quaternion.Euler(0, randomRot, 0);

        GameObject building = Instantiate(prefab, transform.position + offset, rot, transform);
    }



    GameObject GetRandomWeightedBuilding()
    {
        float totalWeight = 0f;
        foreach (var b in buildingTypes)
            totalWeight += b.weight;

        float randomValue = Random.value * totalWeight;

        foreach (var b in buildingTypes)
        {
            if (randomValue < b.weight)
                return b.prefab;

            randomValue -= b.weight;
        }

        return buildingTypes[buildingTypes.Length - 1].prefab;
    }
}
