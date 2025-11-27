using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    [Header("Seed Settings")]
    public int seed = 0;                 
    private System.Random prng;          

    [Header("L-System Settings")]
    public string axiom = "F";
    public int iterations = 3;
    public float stepLength = 1f;
    public float turnAngle = 90f;

    public LRule[] rules;

    [Header("Random Sub-Axiom Settings")]
    public int minSubAxiomLength = 3;
    public int maxSubAxiomLength = 10;
    public string subAxiomCharacters = "FI+-[]x";

    [Header("Prefabs")]
    public GameObject roadStraightPrefab;
    public GameObject roadIntersectionPrefab;

    [Header("Tile Settings")]
    public Vector3 startOffset = Vector3.zero;

    private string generatedString;

    void Start()
    {
        //----------------------------------------
        // Génération de la seed locale
        //----------------------------------------
        int global = GlobalSeedManager.Instance.globalSeed;
        seed = CalculateUniqueSeed(global);

        prng = new System.Random(seed);

        //----------------------------------------
        // Expansion de l'axiome
        //----------------------------------------
        string baseTemplate = axiom;
        axiom = ExpandPlaceholders(baseTemplate);

        Debug.Log($"[{gameObject.name}] Seed locale = {seed}, Axiom = {axiom}");

        GenerateLSystem();
        GenerateRoad();
    }

    // Génère une seed unique à partir de globalSeed + InstanceID
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

    // ----------------------------------------------
    //   L-SYSTEM
    // ----------------------------------------------
    void GenerateLSystem()
    {
        generatedString = axiom;

        for (int i = 0; i < iterations; i++)
        {
            string newString = "";

            foreach (char c in generatedString)
            {
                bool replaced = false;

                foreach (var r in rules)
                {
                    if (c == r.symbol[0])
                    {
                        newString += r.replacement;
                        replaced = true;
                        break;
                    }
                }

                if (!replaced)
                    newString += c;
            }

            generatedString = newString;
        }
    }

    // Expansion des '?', mais en utilisant la seed locale
    string ExpandPlaceholders(string template)
    {
        string result = "";

        foreach (char c in template)
        {
            if (c == '?')
            {
                int length = prng.Next(minSubAxiomLength, maxSubAxiomLength + 1);

                for (int i = 0; i < length; i++)
                {
                    int index = prng.Next(0, subAxiomCharacters.Length);
                    result += subAxiomCharacters[index];
                }
            }
            else
            {
                result += c;
            }
        }

        return result;
    }

    // ----------------------------------------------
    //   GÉNÉRATION DES ROUTES
    // ----------------------------------------------
    void GenerateRoad()
    {
        Vector3 currentPos = transform.position + startOffset;
        Vector3 direction = Vector3.forward;

        Stack<(Vector3 pos, Vector3 dir)> stateStack = new Stack<(Vector3, Vector3)>();

        foreach (char c in generatedString)
        {
            switch (c)
            {
                case 'F':
                    Instantiate(
                        roadStraightPrefab,
                        currentPos,
                        Quaternion.LookRotation(direction),
                        transform
                    );
                    currentPos += direction * stepLength;
                    break;

                case 'I':
                    Instantiate(
                        roadIntersectionPrefab,
                        currentPos,
                        Quaternion.LookRotation(direction),
                        transform
                    );
                    currentPos += direction * stepLength;
                    break;

                case '+':
                    direction = Quaternion.Euler(0, turnAngle, 0) * direction;
                    break;

                case '-':
                    direction = Quaternion.Euler(0, -turnAngle, 0) * direction;
                    break;

                case 'x':
                    direction = Quaternion.Euler(0, turnAngle * 2, 0) * direction;
                    break;

                case '[':
                    stateStack.Push((currentPos, direction));
                    break;

                case ']':
                    (currentPos, direction) = stateStack.Pop();
                    break;
            }
        }
    }
}

[System.Serializable]
public class LRule
{
    public string symbol;
    public string replacement;
}
