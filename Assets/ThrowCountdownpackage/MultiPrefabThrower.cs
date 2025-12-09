using System.Collections.Generic;
using UnityEngine;

public class MultiPrefabThrower : MonoBehaviour
{
    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Prefab List")]
    public List<GameObject> prefabs = new List<GameObject>();

    [Header("Spawn Interval")]
    public float spawnInterval = 1.0f;

    [Tooltip("Throw Force")]
    public float throwForce = 5f;

    [Tooltip("Throw Direction")]
    public Vector3 localThrowDirection = Vector3.down;

    public bool randomPrefab = true;

    public bool autoStart = true;

    private float timer = 0f;
    private bool isSpawning = false;
    private int currentIndex = 0;

    private void Start()
    {
        isSpawning = autoStart;
    }

    private void Update()
    {
        if (!isSpawning)
            return;

        if (prefabs == null || prefabs.Count == 0)
            return; 

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnAndThrow();
        }
    }

    private void SpawnAndThrow()
    {

        FindObjectOfType<HorizontalMover>()?.PlayThrowAnimation();

        GameObject prefabToSpawn = null;

        if (randomPrefab)
        {
            int index = Random.Range(0, prefabs.Count);
            prefabToSpawn = prefabs[index];
        }
        else
        {
            prefabToSpawn = prefabs[currentIndex];
            currentIndex = (currentIndex + 1) % prefabs.Count; 
        }

        if (prefabToSpawn == null)
            return;


        Transform t = spawnPoint != null ? spawnPoint : transform;
        Vector3 pos = t.position;
        Quaternion rot = t.rotation;

        GameObject instance = Instantiate(prefabToSpawn, pos, rot);

 
        Vector3 worldDir = t.TransformDirection(localThrowDirection.normalized);

        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(worldDir * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning($"Prefab {prefabToSpawn.name} No Rigidbody");
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        timer = 0f;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}
