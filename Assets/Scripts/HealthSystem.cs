using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float HP = 100f;

    public void TakeDamage(float dmgValue)
    {
        HP = HP - dmgValue;
        Debug.Log(gameObject.name + " je primio štetu. Trenutni HP: " + HP);
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
