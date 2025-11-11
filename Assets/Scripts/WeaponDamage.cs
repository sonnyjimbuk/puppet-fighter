using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 20f;
    public float attackRadius = 0.8f;
    public float hitCooldown = 0.4f;
    public LayerMask hitLayers; // include "Player"
    public Transform hitPoint;
    public bool showGizmos = true;

    [Header("Sound")]
    public AudioClip hitSound;
    private AudioSource audioSource;

    private float lastHitTime;
    private bool isAttacking = false;
    private GameObject currentHolder;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (!isAttacking || Time.time - lastHitTime < hitCooldown)
            return;

        if (hitPoint == null)
        {
            Debug.LogWarning($"⚠ {name}: Missing hitPoint reference!");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(hitPoint.position, attackRadius, hitLayers);

        foreach (Collider hit in hits)
        {
            if (IsSelf(hit)) continue;

            var health = hit.GetComponentInParent<MarionetteHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                lastHitTime = Time.time;

                if (hitSound != null)
                    audioSource.PlayOneShot(hitSound);

                Debug.Log($"⚔️ {currentHolder?.name ?? "Unknown"} hit {hit.transform.root.name} (damage {damage})");
                break;
            }
        }
    }

    bool IsSelf(Collider col)
    {
        // 获取持有者
        if (currentHolder == null)
        {
            var pick = GetComponent<PickableItem>();
            if (pick != null)
                currentHolder = pick.currentHolder;
        }
        if (currentHolder == null) return false;

        // 比较根对象是否相同
        Transform myRoot = currentHolder.transform.root;
        Transform colRoot = col.transform.root;

        bool isSelf = (myRoot == colRoot);
        if (isSelf)
        {
            Debug.Log($"🧍 {name}: Skipped self-collision with {colRoot.name}");
        }

        return isSelf;
    }

    public void StartAttack()
    {
        isAttacking = true;
        lastHitTime = 0f;
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos || hitPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPoint.position, attackRadius);
    }
}

