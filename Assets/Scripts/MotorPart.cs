using UnityEngine;

public class MotorPart : MonoBehaviour
{
    [Header("Motor Part")]
    public string partName = "Motor Part";

    private bool collected = false;

    public void Collect()
    {
        if (collected)
            return;

        collected = true;

        Debug.Log("Collected motor part: " + partName);

        if (MotorPartManager.Instance != null)
        {
            MotorPartManager.Instance.CollectPart(this);
        }

        Destroy(gameObject);
    }
}