using UnityEngine;
using TMPro;
using System.Collections;

public class HitMarkerUI : MonoBehaviour
{
    public TextMeshProUGUI hitMarkerText;
    public float showDuration = 0.12f;

    private Coroutine currentRoutine;

    private void Start()
    {
        if (hitMarkerText != null)
        {
            hitMarkerText.gameObject.SetActive(false);
        }
    }

    public void ShowHit()
    {
        if (hitMarkerText == null)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(
            ShowHitRoutine()
        );
    }

    private IEnumerator ShowHitRoutine()
    {
        hitMarkerText.text = "X";
        hitMarkerText.gameObject.SetActive(true);

        yield return new WaitForSeconds(showDuration);

        hitMarkerText.gameObject.SetActive(false);
    }
}