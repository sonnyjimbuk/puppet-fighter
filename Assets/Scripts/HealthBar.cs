using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;   // link to the red bar
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        fillImage.fillAmount = currentHealth / maxHealth;
    }

    public void TakeDamage(float amount)
    {
        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }
}
