using UnityEngine;
// Use the CreateAssetMenu attribute to allow creating instances of this ScriptableObject from the Unity Editor.

public enum BIOME
{
    MOUNTAINS,
    BEACH,
    OCEAN,
   
    SNOW,
    TAIGA,
    GRASSLAND,

    DEFAULT,
}
[CreateAssetMenu(fileName = "Data", menuName = "Biome", order = 1)]
public class Biomes : ScriptableObject
{
    public Color biomeColor = Color.black;
    public BIOME biomeType = BIOME.DEFAULT;
}
