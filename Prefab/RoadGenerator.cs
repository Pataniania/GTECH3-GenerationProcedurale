using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    [Header("L-System Settings")]
    public string axiom = "F";
    public int iterations = 3;
    public float stepLength = 1f;
    public float turnAngle = 90f;

    public LRule[] rules;

    [Header("Prefabs")]
    public GameObject roadStraightPrefab;
    public GameObject roadIntersectionPrefab;

    [Header("Tile Settings")]
    public Vector3 startOffset = Vector3.zero;
    public float roadHeight = 0f; // hauteur Y personnalisée

    private string generatedString;

    void Start()
    {
        GenerateLSystem();
        GenerateRoad();
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

    // ----------------------------------------------
    //   GÉNÉRATION DES ROUTES
    // ----------------------------------------------
    void GenerateRoad()
    {
        Vector3 currentPos = transform.position + startOffset;
        currentPos.y = roadHeight;

        Vector3 direction = Vector3.forward;
        Vector3 lastDirection = direction;

        Stack<(Vector3 pos, Vector3 dir)> stateStack = new Stack<(Vector3, Vector3)>();

        foreach (char c in generatedString)
        {
            switch (c)
            {
                case 'F':
                    {
                        // La route doit être centrée entre currentPos et newPos
                        Vector3 forward = direction.normalized;

                        // 1) Position exacte du centre du segment
                        Vector3 centerPos = currentPos + forward * (stepLength * 0.5f);
                        centerPos.y = roadHeight;

                        // 2) Intersection ?
                        bool isIntersection = lastDirection != direction;
                        GameObject prefabToUse = isIntersection ? roadIntersectionPrefab : roadStraightPrefab;

                        // 3) Spawn EXACTEMENT AU CENTRE
                        Instantiate(
                            prefabToUse,
                            centerPos,
                            Quaternion.LookRotation(forward),
                            transform
                        );

                        // 4) Avancer pour le prochain segment
                        currentPos += forward * stepLength;
                        currentPos.y = roadHeight;

                        lastDirection = direction;
                    }
                    break;


                case '+':
                    direction = Quaternion.Euler(0, turnAngle, 0) * direction;
                    break;

                case '-':
                    direction = Quaternion.Euler(0, -turnAngle, 0) * direction;
                    break;

                case '[':
                    stateStack.Push((currentPos, direction));
                    break;

                case ']':
                    (currentPos, direction) = stateStack.Pop();
                    currentPos.y = roadHeight;
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
