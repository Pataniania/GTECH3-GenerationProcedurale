using UnityEngine;

public class GlobalSeedManager : MonoBehaviour
{
    public static GlobalSeedManager Instance;

    [Header("Global Seed")]
    public int globalSeed = 12345;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
