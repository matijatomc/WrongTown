using UnityEngine;
using TMPro;

public class MotorPartsUI : MonoBehaviour
{
    public TextMeshProUGUI motorPartsText;

    private void Start()
    {
        if (MotorPartManager.Instance == null)
        {
            Debug.LogWarning("MotorPartManager not found!");
            return;
        }

        MotorPartManager.Instance.OnPartsChanged += UpdateText;

        UpdateText(
            MotorPartManager.Instance.GetCollectedParts(),
            MotorPartManager.Instance.GetTotalParts()
        );
    }

    private void OnDestroy()
    {
        if (MotorPartManager.Instance != null)
        {
            MotorPartManager.Instance.OnPartsChanged -= UpdateText;
        }
    }

    private void UpdateText(int collected, int total)
    {
        motorPartsText.text = "MOTOR PARTS: " + collected + " / " + total;
    }
}