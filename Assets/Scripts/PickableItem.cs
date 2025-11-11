using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PickableItem : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip pickUpSound;   // Sound when picked up
    public AudioClip dropSound;     // Sound when dropped

    [HideInInspector] public GameObject currentHolder; // Track who is holding this item

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
    }

    // ✅ Called when the item is picked up
    public void PlayPickUpSound()
    {
        if (pickUpSound != null)
            audioSource.PlayOneShot(pickUpSound);
    }

    // ✅ Called when the item is dropped
    public void PlayDropSound()
    {
        if (dropSound != null)
            audioSource.PlayOneShot(dropSound);
    }
}
