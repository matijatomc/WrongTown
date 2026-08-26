using UnityEngine;
using System;

public class MotorPartManager : MonoBehaviour
{
    public static MotorPartManager Instance;

    [Header("Motor Parts")]
    public int totalParts = 8;

    [Header("Game Finish")]
    public GameObject motorcycle;

    private int collectedParts = 0;

    public event Action<int, int> OnPartsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectPart(MotorPart part)
    {
        collectedParts++;

        Debug.Log(
            "Motor part collected: " +
            part.partName +
            " | Progress: " +
            collectedParts +
            "/" +
            totalParts
        );

        OnPartsChanged?.Invoke(collectedParts, totalParts);

        if (collectedParts >= totalParts)
        {
            AllPartsCollected();
        }
    }

    private void AllPartsCollected()
    {
        Debug.Log("All motor parts collected!");

        if (motorcycle != null)
        {
            motorcycle.SetActive(true);
        }
    }

    public int GetCollectedParts()
    {
        return collectedParts;
    }

    public int GetTotalParts()
    {
        return totalParts;
    }
}