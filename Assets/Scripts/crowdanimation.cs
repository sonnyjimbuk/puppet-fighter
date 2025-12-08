using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] frames; // Array to hold your animation frames
    public float frameRate = 0.1f; // Time in seconds between frames
    public bool loop = true; // Whether the animation should loop

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0]; // Set initial frame
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer -= frameRate; // Reset timer
            currentFrameIndex++;

            if (currentFrameIndex >= frames.Length)
            {
                if (loop)
                {
                    currentFrameIndex = 0; // Loop back to the start
                }
                else
                {
                    currentFrameIndex = frames.Length - 1; // Stop at the last frame
                    enabled = false; // Disable script if not looping and at end
                }
            }

            if (frames.Length > 0)
            {
                spriteRenderer.sprite = frames[currentFrameIndex]; // Update sprite
            }
        }
    }
}