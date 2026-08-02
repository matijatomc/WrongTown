using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float HP = 100f;

    public void TakeDamage(float dmgValue)
    {
        HP = HP - dmgValue;
        if (HP <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        Debug.Log("Died");
        Destroy(gameObject);
    }
}
