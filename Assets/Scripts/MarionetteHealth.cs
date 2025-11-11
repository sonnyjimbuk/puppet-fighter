using UnityEngine;
using UnityEngine.UI;

public class MarionetteHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("UI Settings")]
    public Slider healthBar;
    public Canvas worldCanvas; // optional, follow player head

    [Header("Hit Feedback")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    public GameObject hitEffect;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();

        if (healthBar != null)
            healthBar.maxValue = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (healthBar != null)
            healthBar.value = currentHealth;

        // play hit sound
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        // spawn hit effect
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position + Vector3.up * 1.5f, Quaternion.identity);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        Debug.Log($"{gameObject.name} has died!");
        // optional: ragdoll logic / disable control
    }

    void Update()
    {
        // optional: keep healthbar facing camera
        if (worldCanvas != null && Camera.main != null)
            worldCanvas.transform.LookAt(Camera.main.transform);
    }
}
